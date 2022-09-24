namespace Chat.Web.Models
{
    public class ChatMember
    {
        public int Id { get; set; }
        public Room SomeRoom { get; set; }
        public ApplicationUser SomeUser { get; set; }
        public bool IsPrivate { get; set; }
    }
}
