using LMPE_API.DAL;
using LMPE_API.Models;
using WebPush;

namespace LMPE_API.Services
{
    public class PushService
    {
        private readonly PushDal _dal;
        private readonly string _vapidPublic = "BH3IbEl1dPjulBkExkqNjA4QpojoTr2H5XSBvB4KNAKtIjd1_TKIroxO7lFcmDTbmSoZrN-BXNSX0pzY-OOaeg8";
        private readonly string _vapidPrivate = "eiEy-Y_hMssukaIgByHvZpMuLnVbjgDjRlBGPDSftio";

        public PushService(PushDal dal)
        {
            _dal = dal;
        }

        public void SendToUser(long userId, string message)
        {
            var subscriptions = _dal.GetByUserId(userId);
            var webPush = new WebPushClient();

            foreach (var sub in subscriptions)
            {
                var pushSub = new WebPush.PushSubscription
                {
                    Endpoint = sub.Endpoint,
                    P256DH = sub.P256dh,
                    Auth = sub.Auth
                };

                webPush.SendNotification(pushSub, message, new VapidDetails(
                    "mailto:maxence.coeur@outlook.fr",
                    _vapidPublic,
                    _vapidPrivate
                ));
            }
        }
    }
}
