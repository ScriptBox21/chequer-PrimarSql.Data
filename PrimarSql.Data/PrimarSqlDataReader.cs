﻿using System;
using System.Collections;
using System.Data.Common;
using PrimarSql.Data.Cursor;

namespace PrimarSql.Data
{
    public sealed class PrimarSqlDataReader : DbDataReader
    {
        private readonly ICursor _cursor;

        public override int FieldCount => _cursor.FieldCount;

        public override bool HasRows => _cursor.HasRows;

        public override int RecordsAffected => _cursor.RecordsAffected;

        public override bool IsClosed => _cursor.IsClosed;

        public override int Depth => 0;

        public PrimarSqlDataReader(ICursor cursor)
        {
            _cursor = cursor;
        }
        
        public override object this[int ordinal] => _cursor[ordinal];

        public override object this[string name] => _cursor[name];

        public override string GetName(int ordinal)
        {
            return _cursor.Getname(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return _cursor.GetOrdinal(name);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return _cursor.GetDataTypeName(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return _cursor.GetData(ordinal);
        }

        public override int GetValues(object[] values)
        {
            for (var i = 0; i < FieldCount; i++)
                values[i] = GetValue(i);

            return values.Length;
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotSupportedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotSupportedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            return _cursor.GetFieldType(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotSupportedException();
        }

        public override short GetInt16(int ordinal)
        {
            return (short)GetValue(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return (int)GetValue(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return (long)GetValue(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return (float)GetValue(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return (double)GetValue(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return (decimal)GetValue(ordinal);
        }

        public override bool GetBoolean(int ordinal)
        {
            return (bool)GetValue(ordinal);
        }

        public override string GetString(int ordinal)
        {
            return GetValue(ordinal).ToString();
        }

        public override IEnumerator GetEnumerator()
        {
            return new PrimarSqlEnumerator(this);
        }

        public override bool NextResult()
        {
            return false;
        }

        public override bool Read() => _cursor.Read();
        
        public override bool IsDBNull(int ordinal)
        {
            // TODO: Implement
            return false;
        }

        internal class PrimarSqlEnumerator : IEnumerator
        {
            public object Current => _dataReader._cursor.GetDatas();

            private readonly PrimarSqlDataReader _dataReader;

            public PrimarSqlEnumerator(PrimarSqlDataReader dataReader)
            {
                _dataReader = dataReader;
            }

            public bool MoveNext()
            {
                return _dataReader.Read();
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }
    }
}