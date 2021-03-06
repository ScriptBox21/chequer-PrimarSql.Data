﻿using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;
using PrimarSql.Data.Models.Columns;
using PrimarSql.Data.Utilities;
using PrimarSql.Data.Extensions;

namespace PrimarSql.Data.Processors
{
    internal sealed class ColumnProcessor : BaseProcessor
    {
        private readonly DataTable _schemaTable;
        
        public override IColumn[] Columns { get; }

        public ColumnProcessor(IEnumerable<IColumn> columns)
        {
            Columns = columns.ToArray();
            _schemaTable = DataProviderUtility.GetNewSchemaTable();

            int ordinal = 0;
            foreach (var column in columns)
            {
                string name = string.IsNullOrEmpty(column.Alias) ? column.Name.ToName() : column.Alias;
                _schemaTable.Rows.Add(name, ordinal++, typeof(object), column.Name, false);
            }
        }

        public override DataTable GetSchemaTable()
        {
            return _schemaTable;
        }

        public override object[] Process()
        {
            var jObject = Current.ToJObject();

            return _schemaTable.Rows
                .Cast<DataRow>()
                .Select(dataRow => SelectToken(jObject, (IPart[])dataRow["path"]))
                .Cast<object>()
                .ToArray();
        }

        public override Dictionary<string, AttributeValue> Filter()
        {
            var row = new Dictionary<string, AttributeValue>(Current);
            
            IEnumerable<string> names = _schemaTable.Rows
                .Cast<DataRow>()
                .Select(dataRow => dataRow[SchemaTableColumn.ColumnName].ToString())
                .ToArray();

            foreach (KeyValuePair<string, AttributeValue> kv in row.Where(kv => !names.Contains(kv.Key)))
            {
                row.Remove(kv.Key);
            }

            return row;
        }

        private JToken SelectToken(JToken token, IEnumerable<IPart> parts)
        {
            var currentToken = token;

            foreach (var part in parts)
            {
                switch (part)
                {
                    case IdentifierPart identifierPart when currentToken is JObject jObject:
                        currentToken = jObject[identifierPart.Identifier];
                        break;

                    case IndexPart indexPart when currentToken is JArray jArray:
                        currentToken = jArray[indexPart.Index];
                        break;

                    default:
                        return null;
                }

                if (currentToken == null)
                    return null;
            }

            return currentToken;
        }
    }
}
