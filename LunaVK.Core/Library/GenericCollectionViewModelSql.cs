using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LunaVK.Core.Framework;
using LunaVK.Core.Network;
using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace LunaVK.Core.Library
{
    public class GenericCollectionViewModelSql<T> : IDisposable
        where T : class
    {
        private Type _type;
        private string _filename;
        private SqliteConnection _db;

        public GenericCollectionViewModelSql(string filename, Type type)
        {
            this._filename = filename;
            this._type = type;
            this.OpenDb();
        }

        private T MapToClass(SqliteDataReader reader) 
        {
            T returnedObject = Activator.CreateInstance<T>();
            PropertyInfo[] modelProperties = returnedObject.GetType().GetProperties();

            foreach (var modelProperty in modelProperties)
            {
                ColumnAttribute[] attributes = modelProperty.GetCustomAttributes<ColumnAttribute>(true).ToArray();

                if (attributes.Length > 0 && attributes[0].Name != null)
                {
                    var val = reader[attributes[0].Name];
                    //var val = Convert.ChangeType(reader[attributes[0].Name], modelProperty.PropertyType);
                    modelProperty.SetValue(returnedObject, val, null);
                }
            }
            return returnedObject;
        }

        public void Dispose()
        {
            this._filename = null;
            if (this._db != null)
            {
                this._db.Dispose();
                this._db = null;
            }
        }

        private void OpenDb()
        {
            if (this._db != null)
                return;

            bool needInitialize = false;

            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storeForApplication.FileExists(this._filename))
                {
                    var file = storeForApplication.CreateFile(this._filename);
                    file.Dispose();
                    file = null;
                    needInitialize = true;
                }
            }


            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, this._filename);//C:\Users\Makzz\AppData\Local\Packages\13a70736-ce0a-479f-a29c-52a677e09e2d_ngnfk91xjrjgw\LocalState
            this._db = new SqliteConnection($"Filename={dbpath}");


            if (needInitialize)
                this.CreateDatabase();
        }

        protected void CreateTableSql(string tableName)
        {
            this._db.Open();

            List<string> columns = new List<string>();

            PropertyInfo[] modelProperties = this._type.GetProperties();

            foreach (var modelProperty in modelProperties)
            {
                ColumnAttribute[] attributes = modelProperty.GetCustomAttributes<ColumnAttribute>(true).ToArray();

                if (attributes.Length > 0 && attributes[0].Name != null)
                {
                    string column = string.Format("{0} {1}", attributes[0].Name, GetDbType(modelProperty));

                    if (attributes[0].IsPrimaryKey)
                        column += " PRIMARY KEY";

                    columns.Add(column);
                }
            }

            string tableCommand = string.Format("CREATE TABLE {0} ({1});", tableName, string.Join(", \n", columns));

            SqliteCommand createTable = new SqliteCommand(tableCommand, this._db);

            createTable.ExecuteNonQuery();

            this._db.Close();
        }

        public int InsertItem(string tableName, object item)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            Type type = item.GetType();
            PropertyInfo[] modelProperties = type.GetProperties();
            foreach (var modelProperty in modelProperties)
            {
                ColumnAttribute[] attributes = modelProperty.GetCustomAttributes<ColumnAttribute>(true).ToArray();

                if (attributes.Length > 0 && attributes[0].Name != null)
                {
                    values.Add(attributes[0].Name, modelProperty.GetValue(item));
                }
            }

            return this.InsertNonQuery(tableName, values);
        }

        public int DeleteItem(string tableName, object item)
        {
            string primaryKeyName = "";
            string primaryKeyValue = "";

            Type type = item.GetType();
            PropertyInfo[] modelProperties = type.GetProperties();
            foreach (var modelProperty in modelProperties)
            {
                ColumnAttribute[] attributes = modelProperty.GetCustomAttributes<ColumnAttribute>(true).ToArray();

                if (attributes.Length > 0 && attributes[0].IsPrimaryKey == true)
                {
                    primaryKeyName = attributes[0].Name;
                    primaryKeyValue = modelProperty.GetValue(item).ToString();
                }
            }

            return this.DeleteNonQuery(tableName, primaryKeyName, primaryKeyValue);
        }

        public IReadOnlyList<T> GetItems(string tableName)
        {
            List<T> ret = new List<T>();

            this._db.Open();

            using (SqliteCommand selectCommand = new SqliteCommand("SELECT * FROM " + tableName, this._db))
            {
                SqliteDataReader reader = selectCommand.ExecuteReader();

                while (reader.Read())
                {
                    T item = MapToClass(reader);
                    ret.Add(item);
                }

            }

            this._db.Close();

            return ret;
        }

        protected int InsertNonQuery(string tableName, Dictionary<string, object> values)
        {
            int ret = -1;
            this._db.Open();
            List<string> columns = new List<string>();
            using (SqliteCommand insertCommand = new SqliteCommand() { Connection = this._db })
            {
                foreach (var val in values)
                {
                    columns.Add("@" + val.Key);
                    insertCommand.Parameters.AddWithValue("@" + val.Key, val.Value);
                }
                string tableCommand = string.Format("INSERT INTO {0} VALUES ({1});", tableName, string.Join(", ", columns));

                insertCommand.CommandText = tableCommand;

                try
                {
                    ret = insertCommand.ExecuteNonQuery();

                }
                catch (SqliteException ex)
                {

                }
            }
            this._db.Close();
            return ret;
        }

        protected int DeleteNonQuery(string tableName, string key, string value)
        {
            int ret = -1;
            this._db.Open();

            using (SqliteCommand insertCommand = new SqliteCommand() { Connection = this._db })
            {
                string tableCommand = string.Format("DELETE FROM {0} WHERE {1} = '{2}';", tableName, key, value);

                insertCommand.CommandText = tableCommand;

                try
                {
                    ret = insertCommand.ExecuteNonQuery();

                }
                catch (SqliteException ex)
                {

                }
            }
            this._db.Close();
            return ret;
        }

        private static Type NormalizeNullable(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = ((IEnumerable<Type>)type.GetGenericArguments()).First();
            return type;
        }

        private static string GetDbType(PropertyInfo propertyInfo)
        {
            Type type = NormalizeNullable(propertyInfo.PropertyType);

            if (type == typeof(int) || type == typeof(long) || (type == typeof(bool) || type.GetTypeInfo().IsEnum) || type == typeof(DateTime))
                return "INTEGER";
            if (type == typeof(byte[]))
                return "BLOB";
            if (type == typeof(string))
                return "TEXT";
            if (type == typeof(double))
                return "REAL";
            throw new Exception("Unexpected type " + propertyInfo.PropertyType);
        }

        protected virtual void CreateDatabase() { }
    }
}
