using Accountant.Accounts;
using Accountant.Accounts.Metadata;
using Accountant.Configuration.Storage;
using Dahomey.Json;
using Dahomey.Json.Attributes;
using Microsoft.Data.Sqlite;

using SharedUtils.Generic;
using SharedUtils.Storage;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Accountant.Storage
{
    public class SQLiteStorageProvider : StorageProvider
    {
        private SQLiteStorageConfig Config;
        private string ConnectionString;

        internal SQLiteStorageProvider(AccountantPlugin plugin, StorageConfig config) : base(plugin, config)
        {
            if (config is SQLiteStorageConfig sqliteconfig)
            {
                Config = sqliteconfig;
            }
            else
            {
                throw new ArgumentException($"SQLite Storage Provider was supplied with an invalid config (expected {nameof(SQLiteStorageConfig)}, got {config.GetType().Name})");
            }
        }

        public override async ValueTask<StorageResult> Deinitialise()
        {
            return new StorageResult();
        }

        public override async ValueTask<StorageResult> DeleteAccount(Account acc)
        {
            var conn = new SqliteConnection(ConnectionString);

            try
            {
                long id = acc.Identifier;

                await conn.OpenAsync().ConfigureAwait(false);

                await conn.RunNonQueryAsync("DELETE FROM `users` WHERE `id` = @id", new Dictionary<string, object> { { "id", id } }).ConfigureAwait(false);
                await conn.RunNonQueryAsync("DELETE FROM `user_metadata` WHERE `id` = @id", new Dictionary<string, object> { { "id", id } }).ConfigureAwait(false);

                return new StorageResult();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Deleting account id {acc.Identifier} (uname {acc.Username}) failed: {e}");
                return new StorageResult(e);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
            }
        }

        public override async ValueTask<StorageResult> GetAccount(long id)
        {
            var conn = new SqliteConnection(ConnectionString);
            try
            {
                await conn.OpenAsync().ConfigureAwait(false);

                Account acc = null;

                await conn.RunQueryAsync("SELECT `id`, `username`, `password` FROM users WHERE `id` = @id", async (r) =>
                {
                    if (await r.ReadAsync().ConfigureAwait(false))
                    {
                        long id = r.GetInt64(0);
                        string username = r.GetString(1);
                        string password = r.GetString(2);

                        acc = new Account(Manager, this, id)
                        {
                            Username = username,
                            Password = password
                        };

                    }
                }, new Dictionary<string, object> { { "id", id } }).ConfigureAwait(false);

                if (acc != null)
                {
                    await LoadMetadata(conn, acc).ConfigureAwait(false);
                }

                if (acc == null)
                {
                    return new StorageResult(new EntryNotFoundException("No account by this ID exists."));
                }
                else
                {
                    return new StorageResult(acc);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Loading account id {id} failed: {e}");
                return new StorageResult(e);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
            }
        }

        public override async ValueTask<StorageResult> GetAccount(string name)
        {
            var conn = new SqliteConnection(ConnectionString);

            try
            {
                await conn.OpenAsync().ConfigureAwait(false);

                Account acc = null;

                await conn.RunQueryAsync("SELECT `id`, `username`, `password` FROM `users` WHERE `username` = @username", async (r) =>
                {
                    if (await r.ReadAsync().ConfigureAwait(false))
                    {
                        long id = r.GetInt64(0);
                        string username = r.GetString(1);
                        string password = r.GetString(2);

                        acc = new Account(Manager, this, id)
                        {
                            Username = username,
                            Password = password
                        };
                    }

                }, new Dictionary<string, object>() { { "username", name } }).ConfigureAwait(false);

                if (acc != null)
                    await LoadMetadata(conn, acc).ConfigureAwait(false);

                if (acc == null)
                    return new StorageResult(new EntryNotFoundException("No account by this username exists."));
                else
                    return new StorageResult(acc);

            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Loading account name {name} failed: {e}");
                return new StorageResult(e);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
            }

        }

        private async Task LoadMetadata(SqliteConnection conn, Account acc)
        {
            await conn.RunQueryAsync("SELECT `key`, `value` from user_metadata WHERE `id` = @id", async (r) =>
            {
                Dictionary<string, string> kvs = new Dictionary<string, string>();

                while (await r.ReadAsync().ConfigureAwait(false))
                {
                    string key = r.GetString(0);
                    string val = r.GetString(1);

                    kvs.Add(key, val);
                }

                try
                {
                    acc.DeserializeMetadata(kvs);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError($"Failed to load account metadata for user {acc.Identifier} (uname {acc.Username}):\n{e}");
                }

            }, new Dictionary<string, object> { { "id", acc.Identifier } }).ConfigureAwait(false);
        }

        public override async ValueTask<StorageResult> GetAutologinEntries(string uuid)
        {
            var conn = new SqliteConnection(ConnectionString);

            try
            {
                await conn.OpenAsync().ConfigureAwait(false);

                PlayerAutoLogins ali = new PlayerAutoLogins
                {
                    UUID = uuid
                };

                await conn.RunQueryAsync("SELECT `id` FROM `user_metadata` WHERE `key` = @key AND instr(`value`, @uuid) > 0", async (r) =>
                {
                    while (await r.ReadAsync().ConfigureAwait(false))
                    {
                        long account = r.GetInt64(0);
                        ali.Accounts.Add(account);
                    }
                }, new Dictionary<string, object> { { "uuid", uuid }, { "key", Account.AutoLoginMetaKey } }).ConfigureAwait(false);

                return new StorageResult(ali);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Retrieving autologin entries for uuid {uuid} failed: {e}");
                return new StorageResult(e);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
                await conn.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override async ValueTask<StorageResult> Initialize()
        {
            SqliteConnectionStringBuilder sb = new()
            {
                DataSource = Path.GetFullPath(Path.Combine(Plugin.DataFolder, Config.Database))
            };

            ConnectionString = sb.ConnectionString;

            SqliteConnection conn = new(ConnectionString);

            try
            {
                await conn.OpenAsync().ConfigureAwait(false);

                Plugin.Log.LogSuccess($"Storage provider {nameof(SQLiteStorageProvider)} initialised successfully.");

                await InitializeTables(conn).ConfigureAwait(false);
            }
            catch (SqliteException exc)
            {
                Plugin.Log.LogError($"Failed to initialize Accountant: {nameof(SQLiteStorageProvider)} threw an exception:\n{exc}");
                return new StorageResult(exc);
            }
            finally
            {
                await conn.CloseAsync().ConfigureAwait(false);
                await conn.DisposeAsync().ConfigureAwait(false);
            }

            return new StorageResult();
        }

        private async Task InitializeTables(SqliteConnection connection)
        {
            await connection.RunNonQueryAsync("CREATE TABLE IF NOT EXISTS `users` (`id` INTEGER PRIMARY KEY AUTOINCREMENT, `username` TEXT UNIQUE, `password` TEXT)").ConfigureAwait(false);
            await connection.RunNonQueryAsync("CREATE TABLE IF NOT EXISTS `user_metadata` ( `id` INTEGER, `key` TEXT NOT NULL, `value` TEXT NOT NULL, PRIMARY KEY(`id`,`key`))").ConfigureAwait(false);
        }

        public override async ValueTask<StorageResult> SaveAccount(Account account)
        {
            SqliteConnection conn = new(ConnectionString);

            try
            {
                await conn.OpenAsync().ConfigureAwait(false);

                using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);

                if (account.Identifier == -1)
                {
                    await conn.RunNonQueryAsync("INSERT OR IGNORE INTO users (`username`, `password`) VALUES (@username, @password)", new Dictionary<string, object> { { "username", account.Username }, { "password", account.Password } }).ConfigureAwait(false);

                    long last_id = -1;

                    await conn.RunQueryAsync("SELECT last_insert_rowid()", async (r) =>
                    {
                        if (!await r.ReadAsync().ConfigureAwait(false))
                        {
                            throw new InvalidOperationException("sqlite last_insert_rowid() returned no rows");
                        }
                        else
                        {
                            last_id = r.GetInt64(0);
                        }

                    }).ConfigureAwait(false);

                    if (last_id == -1)
                        throw new InvalidOperationException("RunQuery did not run ReaderCallback.");

                    if (last_id == 0)
                        throw new EntryAlreadyExistsException("An account by this name already exists.");

                    account.Identifier = last_id;
                }
                else
                {
                    await conn.RunNonQueryAsync("UPDATE users SET `username` = @username, `password` = @password WHERE `id` = @id", new Dictionary<string, object> { { "username", account.Username }, { "password", account.Password }, { "id", account.Identifier } }).ConfigureAwait(false);
                }

                //Merge the metadata store here.

                var kvs = account.SerializeMetadata();

                await conn.RunNonQueryAsync("DELETE FROM user_metadata WHERE id = @id", new() { { "id", account.Identifier } }).ConfigureAwait(false);

                foreach(var kv in kvs)
                {
                    await conn.RunNonQueryAsync("INSERT INTO user_metadata (`id`, `key`, `value`) VALUES (@id, @key, @value) ON CONFLICT (`id`, `key`) DO UPDATE SET `value` = @value", new Dictionary<string, object> { { "id", account.Identifier }, { "key", kv.Key }, { "value", kv.Value } }).ConfigureAwait(false);
                }

                await transaction.CommitAsync().ConfigureAwait(false);

                return new StorageResult();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Saving account {account.Identifier} (uname {account.Username}) failed: {e}");
                return new StorageResult(e);
            }
            finally 
            {
                await conn.CloseAsync().ConfigureAwait(false);
                await conn.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
