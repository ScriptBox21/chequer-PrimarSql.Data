﻿using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace PrimarSql.Data.Expressions.Generators
{
    internal partial class ExpressionGenerator
    {
        public TableDescription TableDescription { get; }

        public string IndexName { get; }

        public IExpression Expression { get; }

        public string HashKeyName { get; private set; }

        public string SortKeyName { get; private set; }

        protected GeneratorContext Context;

        public ExpressionGenerator(TableDescription tableDescription, string indexName, IExpression expression)
        {
            TableDescription = tableDescription;
            IndexName = indexName;
            Expression = expression;
        }

        public ExpressionGenerateResult Analyze()
        {
            Context = new GeneratorContext();

            SetPrimaryKey();
            AnalyzeInternal(Expression, null, 0);
            ValidatePrimaryKey();

            return new ExpressionGenerateResult
            {
                HashKey = Context.HashKeys.FirstOrDefault(),
                SortKey = Context.SortKeys.FirstOrDefault(),
                FilterExpression = string.Join(" ", Context.Buffers.Select(b => b?.ToString() ?? string.Empty)),
                ExpressionAttributeNames = Context.AttributeNames,
                ExpressionAttributeValues = Context.AttributeValues
            };
        }

        private List<KeySchemaElement> FindLocalSecondaryIndex(string indexName)
        {
            return TableDescription
                .LocalSecondaryIndexes
                .FirstOrDefault(index => index.IndexName == indexName)?
                .KeySchema;
        }

        private List<KeySchemaElement> FindGlobalSecondaryIndex(string indexName)
        {
            return TableDescription
                .GlobalSecondaryIndexes
                .FirstOrDefault(index => index.IndexName == indexName)?
                .KeySchema;
        }

        private void SetPrimaryKey()
        {
            List<KeySchemaElement> keySchemaElements;

            if (string.IsNullOrEmpty(IndexName))
            {
                keySchemaElements = TableDescription.KeySchema;
            }
            else
            {
                keySchemaElements =
                    FindLocalSecondaryIndex(IndexName) ??
                    FindGlobalSecondaryIndex(IndexName) ??
                    throw new InvalidOperationException($"{TableDescription.TableName} table has no '{IndexName}' Index.");
            }

            foreach (var element in keySchemaElements)
            {
                if (element.KeyType == KeyType.HASH)
                {
                    HashKeyName = element.AttributeName;
                }
                else if (element.KeyType == KeyType.RANGE)
                {
                    SortKeyName = element.AttributeName;
                }
                else
                {
                    throw new NotSupportedException($"KeyType '{element.KeyType.Value}' is not supported.");
                }
            }
        }

        private void ValidatePrimaryKey()
        {
            if (Context.HashKeys.Count > 1)
                Context.HashKeys.Clear();

            if (Context.SortKeys.Count > 1 || (Context.SortKeys.Count == 1 && Context.HashKeys.Count != 1))
                Context.SortKeys.Clear();

            if (Context.HashKeys.Count == 1)
            {
                var hashKey = Context.HashKeys[0];

                for (int i = hashKey.StartToken; i <= hashKey.EndToken; i++)
                {
                    Context.Buffers[i] = null;
                }
            }

            if (Context.SortKeys.Count == 1)
            {
                var sortKey = Context.SortKeys[0];

                for (int i = sortKey.StartToken; i <= sortKey.EndToken; i++)
                {
                    Context.Buffers[i] = null;
                }
            }
        }
    }
}
