using Subscription.Enums;

namespace Subscription.Models
{
    public class SubscriptionModel
    {
        public int Id { get; set; }
        public string Domain { get; set; }        
        public DateTime ValidUntil { get; set; }
        public SubscriptionEnum SubscriptionEnum { get; set; }  
    }
}
