
using Refit;

namespace SigmaApp.API
{

    //this API handles GET and PUT requests to the server so that certain information can be accessed without initialising the SignalR hubconnection

    [Headers("Content-Type: application/json")]
    public interface IUserInfo
    {

        [Get("/api/crypto/GetKey/{userId}")]
        Task<string> GetKey(string userId);

        [Get("/api/crypto/GetUserExists/{userId}")]
        Task<bool> GetUserExists(string userId);

        //[Post("/api/login")]
        //Task<string> LoginUser([Body] User user);
    }
}
