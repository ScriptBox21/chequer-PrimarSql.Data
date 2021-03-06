﻿using System.Data.Common;
using PrimarSql.Data.Providers;

namespace PrimarSql.Data.Planners.Show
{
    internal sealed class ShowTablePlanner : QueryPlanner<EmptyQueryInfo>
    {
        public ShowTablePlanner()
        {
            QueryInfo = new EmptyQueryInfo();
        }

        public override DbDataReader Execute()
        {
            var response = Context.Client.ListTablesAsync().Result;
            var provider = new ListDataProvider();

            provider.AddColumn("name", typeof(string));

            foreach (string tableName in response.TableNames)
            {
                provider.AddRow(tableName);
            }

            return new PrimarSqlDataReader(provider);
        }
    }
}
