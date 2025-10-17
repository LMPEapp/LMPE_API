using LMPE_API.DAL;
using LMPE_API.Models;
using WebPush;
using System.Text.Json;

namespace LMPE_API.Services
{
    public class PushService
    {
        private readonly PushDal _dal;
        private readonly string _vapidPublic = "BJieS9rJZ5dcmEVMOzyjjz4hh-nkIntZ7Zpx61DpirktTSjK9aHfUjw1lNuzFWCPjD-5cR1xj_unleYj3Ru7ySc";
        private readonly string _vapidPrivate = "hKKheGs0lu7pEr1xCFrteLk6EQZUptljHBtWNR_wh6M";

        public PushService(PushDal dal)
        {
            _dal = dal;
        }

        public void SendToUser(long userId, GroupeConversation? groupeConversation)
        {
            if(groupeConversation == null){ Console.WriteLine("PushService.SendToUser : groupeConversation est null ");}
            var subscriptions = _dal.GetByUserId(userId);
            var webPush = new WebPushClient();

            foreach (var sub in subscriptions)
            {
                if (string.IsNullOrWhiteSpace(sub.Endpoint))
                {
                    Console.WriteLine($"Abonnement invalide pour l'utilisateur {userId}, suppression en base");
                    _dal.Delete(sub.Id);
                    continue;
                }

                var pushSub = new WebPush.PushSubscription
                {
                    Endpoint = sub.Endpoint,
                    P256DH = sub.P256dh,
                    Auth = sub.Auth
                };

                // Payload similaire à l'exemple Node.js
                var payload = new
                {
                    notification = new
                    {
                        title = $"Groupe : {groupeConversation.Name}",
                        body = "Vous avez un (des) nouveau(x) message(s)",
                        icon = "/assets/logo.png",
                        vibrate = new[] { 100, 50, 100 },
                        tag = $"Groupe_{groupeConversation.Id}",
                        renotify = true,
                        data = new
                        {
                            dateOfArrival = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            primaryKey = 1,
                            groupId = groupeConversation.Id,
                            groupName = groupeConversation.Name
                        }
                    }
                };

                try
                {
                    webPush.SendNotification(
                        pushSub,
                        JsonSerializer.Serialize(payload),
                        new VapidDetails(
                            "mailto:maxence.coeur@outlook.fr",
                            _vapidPublic,
                            _vapidPrivate
                        )
                    );
                }
                catch (WebPushException ex)
                {
                    // Si l'abonnement n'est plus valide, on le supprime
                    if (ex.StatusCode == System.Net.HttpStatusCode.Gone ||
                        ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"Abonnement expiré pour l'utilisateur {userId}, suppression en base");
                        _dal.Delete(sub.Id);
                    }
                    else
                    {
                        Console.WriteLine($"Erreur push pour {userId}: {ex.Message}");
                    }
                }
            }
        }
    }
}
