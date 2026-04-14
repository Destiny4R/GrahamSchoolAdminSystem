using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemAccess.ServiceRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GrahamSchoolAdminSystemWeb.Hubs
{
    [Authorize]
    public class PaymentNotificationHub : Hub
    {
        private readonly IPermissionService _permissionService;

        public PaymentNotificationHub(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public override async Task OnConnectedAsync()
        {
            // Add users with REPORT permission to the "Approvers" group
            if (Context.User != null)
            {
                var hasReportPerm = await _permissionService.UserHasPermissionAsync(Context.User, SD.Permissions.REPORT);
                if (hasReportPerm)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Approvers");
                }
            }
            await base.OnConnectedAsync();
        }
    }
}
