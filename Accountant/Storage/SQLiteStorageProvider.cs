using Accountant.Accounts;
using Accountant.Configuration.Storage;

using Microsoft.Data.Sqlite;

using Newtonsoft.Json;

using SharedUtils.Generic;
using SharedUtils.Storage;
using SharedUtils.Storage.Exceptions;

using System;
using System.Collections.Generic;
using System.Text;

namespace Accountant.Storage
{
    public class SQLiteStorageProvider : StorageProvider
    {
        private SQLiteStorageConfig Config;
        private string ConnectionString;

        internal SQLiteStorageProvider(AccountantPlugin plugin, StorageConfig config) : base(plugin, config)
        {
            if(config is SQLiteStorageConfig sqliteconfig)
            {
                Config = sqliteconfig;
            }
            else
            {
                throw new ArgumentException($"SQLite Storage Provider was supplied with an invalid config (expected {nameof(SQLiteStorageConfig)}, got {config.GetType().Name})");
            }
        }

        public override StorageResult Deinitialise()
        {
            return new StorageResult();
        }

        public override StorageResult DeleteAccount(Account acc)
        {
            var conn = new SqliteConnection(ConnectionString);
            try
            {
                conn.Open();

                conn.RunNonQuery("DELETE FROM `users` WHERE `id` = @id", new Dictionary<string, object> { { "id", acc.Identifier } });
                conn.RunNonQuery("DELETE FROM `user_metadata` WHERE `id` = @id", new Dictionary<string, object> { { "id", acc.Identifier } });

                return new StorageResult();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Deleting account id {acc.Identifier} (uname {acc.Username}) failed:");
                return new StorageResult(e);
            }
            finally
            {
                conn.Close();
            }
        }

        public override StorageResult GetAccount(long id)
        {
            var conn = new SqliteConnection(ConnectionString);
            try
            {
                conn.Open();

                Account acc = null;

                conn.RunQuery("SELECT `id`, `username`, `password` FROM users WHERE `id` = @id", (r) =>
                {
                    if(r.Read())
                    {
                        long id = r.GetInt64(0);
                        string username = r.GetString(1);
                        string password = r.GetString(2);

                        acc = new Account(this, id)
                        {
                            Username = username,
                            Password = password
                        };

                    }
                }, new Dictionary<string, object> { { "id", id } });

                if (acc != null)
                {
                    LoadMetadata(conn, acc);
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
                conn.Close();
            }
        }

        public override StorageResult GetAccount(string name)
        {
            var conn = new SqliteConnection(ConnectionString);

            try
            {
                conn.Open();

                Account acc = null;

                conn.RunQuery("SELECT `id`, `username`, `password` FROM `users` WHERE `username` = @username", (r) =>
                {
                    if (r.Read())
                    {
                        long id = r.GetInt64(0);
                        string username = r.GetString(1);
                        string password = r.GetString(2);

                        acc = new Account(this, id)
                        {
                            Username = username,
                            Password = password
                        };
                    }

                }, new Dictionary<string, object>() { { "username", name } });

                if (acc != null)
                    LoadMetadata(conn, acc);

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
                conn.Close();
            }

        }

        private void LoadMetadata(SqliteConnection conn, Account acc)
        {
            conn.RunQuery("SELECT `key`, `value` from user_metadata WHERE `id` = @id", (r) =>
            {
                var serializer_settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

                while (r.Read())
                {
                    string key = r.GetString(0);
                    string val = r.GetString(1);

                    object obj;

                    try
                    {
                        obj = JsonConvert.DeserializeObject(val, serializer_settings);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Invalid metadata JSON in account {acc.Identifier} (uname: {acc.Username}) at key {key}: {e.Message}", e);
                    }

                    acc.Metadata.Add(key, obj);

                }
            }, new Dictionary<string, object> { { "id", acc.Identifier } });
        }

        public override StorageResult GetAutologinEntries(string uuid)
        {
            var conn = new SqliteConnection(ConnectionString);

            try
            {
                conn.Open();

                PlayerAutoLogins ali = new PlayerAutoLogins();

                ali.UUID = uuid;

                conn.RunQuery("SELECT `id` FROM `user_metadata` WHERE `key` = @key AND instr(`value`, @uuid) > 0", (r) =>
                {
                    while(r.Read())
                    {
                        long account = r.GetInt64(0);
                        ali.Accounts.Add(account);
                    }
                }, new Dictionary<string, object> { { "uuid", uuid }, { "key", Account.AutoLoginMetaKey } });

                return new StorageResult(ali);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Retrieving autologin entries for uuid {uuid} failed: {e}");
                return new StorageResult(e);
            }
            finally
            {
                conn.Close();
            }
        }

        public override StorageResult Initialize()
        {
            SqliteConnectionStringBuilder sb = new SqliteConnectionStringBuilder
            {
                DataSource = Config.Database
            };

            ConnectionString = sb.ConnectionString;

            SqliteConnection conn = new SqliteConnection(ConnectionString);

            try
            {
                conn.Open();
            }
            catch (SqliteException exc)
            {
                Plugin.Log.LogError($"Failed to initialize Accountant: {nameof(SQLiteStorageProvider)} threw an exception:\n{exc}");
                return new StorageResult(exc);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return new StorageResult();
        }

        private void InitialiseTables(SqliteConnection connection)
        {
            connection.RunNonQuery("CREATE TABLE IF NOT EXISTS `users` (`id` INTEGER PRIMARY KEY AUTOINCREMENT, `username` TEXT UNIQUE, `password` TEXT)");
            connection.RunNonQuery("CREATE TABLE IF NOT EXISTS `user_metadata` ( `id` INTEGER, `key` TEXT NOT NULL, `value` TEXT NOT NULL, PRIMARY KEY(`id`,`key`))");
        }

        public override StorageResult SaveAccount(Account account)
        {
            SqliteConnection conn = new SqliteConnection(ConnectionString);

            try
            {
                conn.Open();

                var transaction = conn.BeginTransaction();

                if (account.Identifier == -1)
                {
                    conn.RunNonQuery("INSERT OR IGNORE INTO users (`username`, `password`) VALUES (@username, @password)", new Dictionary<string, object> { { "username", account.Username }, { "password", account.Password } });
                    long last_id = -1;
                    conn.RunQuery("SELECT last_insert_rowid()", (r) =>
                    {
                        if (!r.Read())
                        {
                            throw new InvalidOperationException("sqlite last_insert_rowid() returned no rows");
                        }
                        else
                        {
                            last_id = r.GetInt64(0);
                        }

                    });

                    if (last_id == -1)
                        throw new InvalidOperationException("RunQuery did not run ReaderCallback.");

                    if (last_id == 0)
                        throw new EntryAlreadyExistsException("An account by this name already exists.");

                    account.Identifier = last_id;
                }
                else
                {
                    conn.RunNonQuery("UPDATE users SET `username` = @username, `password` = @password WHERE `id` = @id", new Dictionary<string, object> { { "username", account.Username }, { "password", account.Password }, { "id", account.Identifier } });
                }

                //Merge the metadata store here.

                var meta = account.Metadata;

                var serializer_settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

                foreach(var kv in meta)
                {
                    string value_string = JsonConvert.SerializeObject(kv.Value);
                    conn.RunNonQuery("INSERT INTO user_metadata (`id`, `key`, `value`) VALUES (@id, @key, @value) ON CONFLICT (`id`, `key`) DO UPDATE SET `value` = @value", new Dictionary<string, object> { { "id", account.Identifier }, { "key", kv.Key }, { "value", value_string } });
                }

                transaction.Commit();

                return new StorageResult();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Saving account {account.Identifier} (uname {account.Username}) failed: {e}");
                return new StorageResult(e);
            }
            finally 
            {
                conn.Close();
            }
        }
    }
}
