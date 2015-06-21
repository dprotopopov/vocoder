using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace vocoder.DatabaseLibrary
{
    public class Database
    {
        /// <summary>
        ///     Строка для соединения с базой данных
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Коннектор к базе данных
        /// </summary>
        public SQLiteConnection Connection { get; set; }

        /// <summary>
        ///     Название файла базы данных
        /// </summary>
        public string DatabaseFilename { get; set; }

        public static T ConvertTo<T>(object obj)
        {
            return (T) TypeDescriptor.GetConverter(obj).ConvertTo(obj, typeof (T));
        }

        public bool CreateDatabaseIfNotExists()
        {
            if (File.Exists(DatabaseFilename)) return false;
            SQLiteConnection.CreateFile(DatabaseFilename);
            Connect();
            return true;
        }

        public IEnumerable<Record> Load(Record maskRecord, string compare = "=")
        {
            var list = new List<Record>();
            try
            {
                Connection.Open();
                using (SQLiteCommand command = Connection.CreateCommand())
                {
                    string where = String.Join(" AND ", maskRecord.GetType()
                        .GetProperties()
                        .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                        .Select(prop => String.Format("{0}{1}@{0}", prop.Name, compare)));
                    command.CommandText = String.IsNullOrWhiteSpace(where)
                        ? String.Format("SELECT * FROM {0}", maskRecord.GetType().Name)
                        : String.Format("SELECT * FROM {0} WHERE {1}", maskRecord.GetType().Name, where);
                    Debug.WriteLine(command.CommandText);
                    foreach (
                        PropertyInfo prop in
                            maskRecord.GetType()
                                .GetProperties()
                                .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                        )
                    {
                        command.Parameters.Add(new SQLiteParameter(String.Format("@{0}", prop.Name),
                            prop.GetValue(maskRecord, null)));
                    }
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        object record = Activator.CreateInstance(maskRecord.GetType());
                        for (int i = 0; i < reader.FieldCount; i++)
                            record.GetType()
                                .GetProperty(reader.GetName(i))
                                .SetValue(record, reader[i], null);
                        list.Add((Record) record);
                    }
                }
            }
            finally
            {
                Connection.Close();
            }
            return list;
        }

        public static bool IsNullOrEmpty(object value)
        {
            if (value is string)
                return string.IsNullOrEmpty(value as string);
            if (value is int)
                return (int) value == 0;
            return value == null;
        }

        /// <summary>
        ///     Установление соединения с базой данных
        /// </summary>
        public virtual void Connect()
        {
            if (Connection != null) return;
            ConnectionString = String.Format("data source={0}", DatabaseFilename);
            Connection = new SQLiteConnection(ConnectionString);
        }

        /// <summary>
        ///     Удаление из базы данных всех записей, удовлетворяющих указанному критерию
        /// </summary>
        /// <param name="maskRecord"></param>
        /// <param name="compare"></param>
        public void Delete(Record maskRecord, string compare = "=")
        {
            try
            {
                Connection.Open();
                using (SQLiteCommand command = Connection.CreateCommand())
                {
                    string where = String.Join(" AND ",
                        maskRecord.GetType()
                            .GetProperties()
                            .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                            .Select(prop => String.Format("{0}{1}@{0}", prop.Name, compare)));
                    command.CommandText = String.IsNullOrWhiteSpace(where)
                        ? String.Format("DELETE FROM {0}", maskRecord.GetType().Name)
                        : String.Format("DELETE FROM {0} WHERE {1}", maskRecord.GetType().Name, where);
                    Debug.WriteLine(command.CommandText);
                    foreach (
                        PropertyInfo prop in
                            maskRecord.GetType()
                                .GetProperties()
                                .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                        )
                        command.Parameters.Add(new SQLiteParameter(String.Format("@{0}", prop.Name),
                            prop.GetValue(maskRecord, null)));
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                Connection.Close();
            }
        }

        /// <summary>
        ///     Получение следующей записи, удовлетворяющей критерию, и удаление полученной записи из базы данных
        /// </summary>
        /// <param name="maskRecord"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public Record GetNext(Record maskRecord, string compare = "=")
        {
            var record = new Record();
            Debug.WriteLine(ConnectionString);
            try
            {
                Connection.Open();
                using (SQLiteCommand command = Connection.CreateCommand())
                {
                    string where = String.Join(" AND ",
                        maskRecord.GetType()
                            .GetProperties()
                            .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                            .Select(prop => String.Format("{0}{1}@{0}", prop.Name, compare)));
                    command.CommandText = String.IsNullOrWhiteSpace(where)
                        ? String.Format("SELECT * FROM {0} LIMIT 1", maskRecord.GetType().Name)
                        : String.Format("SELECT * FROM {0} WHERE {1} LIMIT 1", maskRecord.GetType().Name, where);
                    Debug.WriteLine(command.CommandText);
                    foreach (
                        PropertyInfo prop in
                            maskRecord.GetType()
                                .GetProperties()
                                .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                        )
                        command.Parameters.Add(new SQLiteParameter(String.Format("@{0}", prop.Name),
                            prop.GetValue(maskRecord, null)));
                    SQLiteDataReader reader = command.ExecuteReader();
                    if (!reader.HasRows)
                        throw new ObjectNotFoundException();
                    while (reader.Read())
                        for (int i = 0; i < reader.FieldCount; i++)
                            record.GetType()
                                .GetProperty(reader.GetName(i))
                                .GetValue(reader[i], null);
                }
                using (SQLiteCommand command = Connection.CreateCommand())
                {
                    string where = String.Join(" AND ",
                        maskRecord.GetType()
                            .GetProperties()
                            .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                            .Select(prop => String.Format("{0}{1}@{0}", prop.Name, compare)));
                    command.CommandText = String.IsNullOrWhiteSpace(where)
                        ? String.Format("DELETE FROM {0}", maskRecord.GetType().Name)
                        : String.Format("DELETE FROM {0} WHERE {1}", maskRecord.GetType().Name, where);
                    Debug.WriteLine(command.CommandText);
                    foreach (
                        PropertyInfo prop in
                            maskRecord.GetType()
                                .GetProperties()
                                .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                        )
                        command.Parameters.Add(new SQLiteParameter(String.Format("@{0}", prop.Name),
                            prop.GetValue(record, null)));
                }
            }
            finally
            {
                Connection.Close();
            }
            return record;
        }

        /// <summary>
        ///     Добавление записей в базу данных.
        ///     При совпадении ключевых полей с существующими записями, существующая запись перезаписывается
        /// </summary>
        /// <param name="records">Список добавляемых записей</param>
        /// <returns>Количество добавленных или изменённых записей</returns>
        public int InsertOrReplace(IEnumerable<Record> records)
        {
            int insertOrReplace = 0;
            try
            {
                Connection.Open();
                using (SQLiteCommand command = Connection.CreateCommand())
                {
                    foreach (Record record in records)
                    {
                        command.CommandText =
                            String.Format("INSERT OR REPLACE INTO {0}({1}) VALUES ({2})", record.GetType().Name,
                                String.Join(",",
                                    record.GetType()
                                        .GetProperties()
                                        .Where(prop => !IsNullOrEmpty(prop.GetValue(record, null)))
                                        .Select(prop => prop.Name)),
                                String.Join(",",
                                    record.GetType()
                                        .GetProperties()
                                        .Where(prop => !IsNullOrEmpty(prop.GetValue(record, null)))
                                        .Select(prop => "@" + prop.Name)));
                        Debug.WriteLine(command.CommandText);
                        foreach (
                            PropertyInfo prop in
                                record.GetType()
                                    .GetProperties()
                                    .Where(prop => !IsNullOrEmpty(prop.GetValue(record, null)))
                            )
                            command.Parameters.Add(new SQLiteParameter(String.Format("@{0}", prop.Name),
                                prop.GetValue(record, null)));
                        insertOrReplace += command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                Connection.Close();
            }
            return insertOrReplace;
        }

        public int InsertOrReplace(Record record)
        {
            int insertOrReplace = 0;
            try
            {
                Connection.Open();
                using (SQLiteCommand command = Connection.CreateCommand())
                {
                    command.CommandText =
                        String.Format("INSERT OR REPLACE INTO {0}({1}) VALUES ({2})", record.GetType().Name,
                            String.Join(",",
                                record.GetType()
                                    .GetProperties()
                                    .Where(prop => !IsNullOrEmpty(prop.GetValue(record, null)))
                                    .Select(prop => prop.Name)),
                            String.Join(",",
                                record.GetType()
                                    .GetProperties()
                                    .Where(prop => !IsNullOrEmpty(prop.GetValue(record, null)))
                                    .Select(prop => "@" + prop.Name)));
                    Debug.WriteLine(command.CommandText);
                    foreach (
                        PropertyInfo prop in
                            record.GetType()
                                .GetProperties()
                                .Where(prop => !IsNullOrEmpty(prop.GetValue(record, null)))
                        )
                        command.Parameters.Add(new SQLiteParameter(String.Format("@{0}", prop.Name),
                            prop.GetValue(record, null)));
                    insertOrReplace = command.ExecuteNonQuery();
                }
            }
            finally
            {
                Connection.Close();
            }
            return insertOrReplace;
        }

        /// <summary>
        ///     Выполнение SQL кода в базе данных
        /// </summary>
        /// <param name="commandText">Текст SQL кода</param>
        /// <returns>Код, возвращаемый командой ExecuteNonQuery</returns>
        public int ExecuteNonQuery(string commandText)
        {
            int executeNonQuery = 0;
            try
            {
                Connection.Open();
                using (SQLiteCommand command = Connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    Debug.WriteLine(command.CommandText);
                    executeNonQuery = command.ExecuteNonQuery();
                }
            }
            finally
            {
                Connection.Close();
            }
            return executeNonQuery;
        }

        public int InsertIfNotExists(Record record)
        {
            return Exists(record) ? 0 : InsertOrReplace(record);
        }

        /// <summary>
        ///     Проверка существования записей в базе данных с указанным критерием
        /// </summary>
        /// <param name="maskRecord">Параметры критерия поиска записей</param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public bool Exists(Record maskRecord, string compare = "=")
        {
            bool value = false;
            try
            {
                Connection.Open();
                using (SQLiteCommand command = Connection.CreateCommand())
                {
                    string where = String.Join(" AND ",
                        maskRecord.GetType()
                            .GetProperties()
                            .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                            .Select(prop => String.Format("{0}{1}@{0}", prop.Name, compare)));
                    command.CommandText = String.IsNullOrWhiteSpace(where)
                        ? String.Format("SELECT * FROM {0} LIMIT 1", maskRecord.GetType().Name)
                        : String.Format("SELECT * FROM {0} WHERE {1} LIMIT 1", maskRecord.GetType().Name, where);
                    Debug.WriteLine(command.CommandText);
                    foreach (
                        PropertyInfo prop in
                            maskRecord.GetType()
                                .GetProperties()
                                .Where(prop => !IsNullOrEmpty(prop.GetValue(maskRecord, null)))
                        )
                        command.Parameters.Add(new SQLiteParameter(String.Format("@{0}", prop.Name),
                            prop.GetValue(maskRecord, null)));
                    SQLiteDataReader reader = command.ExecuteReader();
                    value = reader.HasRows;
                }
            }
            finally
            {
                Connection.Close();
            }
            return value;
        }
    }
}