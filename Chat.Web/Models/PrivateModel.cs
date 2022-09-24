using System.Collections.Generic;
namespace Chat.Web.Models
{
    public class PrivateModel
    {
        public int RoomId { get; set; }
        public ApplicationUser FirstUserId { get; set; }
        public ApplicationUser SecondUserId { get; set; }
    }
}
