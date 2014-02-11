using PintheCloud.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public interface AccountSpaceRelationManager
    {
        void SetAccountSpaceRelationWorker(AccountSpaceRelationWorker CurrentAccountSpaceRelationWorker);
        Task<bool> LikeAysnc(string spaceId, bool whether);
        Task<bool> IsLikeAsync(string space_id);
    }
}
