using PintheCloud.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public class AccountSpaceRelationManagerImplement : AccountSpaceRelationManager
    {
        private AccountSpaceRelationWorker CurrentAccountSpaceRelationWorker = null;
        public void SetAccountSpaceRelationWorker(AccountSpaceRelationWorker CurrentAccountSpaceRelationWorker)
        {
            this.CurrentAccountSpaceRelationWorker = CurrentAccountSpaceRelationWorker;
        }

        private bool IsLikeProcessing { get; set; }  // Mutex


        public async Task<bool> LikeAysnc(string spaceId, bool whether)
        {
            // If like is not processing, go
            // Otherwise, don't go
            if (!this.IsLikeProcessing)
            {
                this.IsLikeProcessing = true;  // Mutex
                bool likeSuccess = await this.CurrentAccountSpaceRelationWorker
                        .LikeAsync(App.CurrentAccountManager.GetCurrentAcccount().account_platform_id, spaceId, whether);
                this.IsLikeProcessing = false;  // Mutex

                if (likeSuccess)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
    }
}
