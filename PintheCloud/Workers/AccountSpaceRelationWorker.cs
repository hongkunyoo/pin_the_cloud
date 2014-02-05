using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Workers
{
    public abstract class AccountSpaceRelationWorker
    {
        // If true, Like, Insert relation
        // Otherwise, Not like, Delete relation
        public async Task<bool> LikeAsync(string accountId, string spaceId, bool whether)
        {
            if (whether)
            {
                try
                {
                    // TODO ++ Like Number
                    await App.MobileService.GetTable<AccountSpaceRelation>().InsertAsync(new AccountSpaceRelation(accountId, spaceId));
                }
                catch (MobileServiceInvalidOperationException)
                {
                    return false;
                }
            }
            else
            {
                // TODO -- Like Number
                string json = @"{'accountId':'" + accountId + "','spaceId':'" + spaceId + "'}";
                JToken jToken = JToken.Parse(json);
                try
                {
                    // Load near spaces use custom api in server script
                    await App.MobileService.InvokeApiAsync("delete_account_space_relation_without_id", jToken);
                }
                catch (MobileServiceInvalidOperationException)
                {
                    return false;
                }
            }
            return true;
        }



        // Get whether the account likes the space
        public async Task<AccountSpaceRelation> IsLikeAsync(string account_id, string space_id)
        {
            MobileServiceCollection<AccountSpaceRelation, AccountSpaceRelation> relations = null;
            try
            {
                // Load current account's spaces
                relations = await App.MobileService.GetTable<AccountSpaceRelation>()
                    .Where(s => s.account_id == account_id && s.space_id == space_id)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
            }
            if (relations.Count > 0)
                return relations.First();
            else
                return null;
        }
    }
}
