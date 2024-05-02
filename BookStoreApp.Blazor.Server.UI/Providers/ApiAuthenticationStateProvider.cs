using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookStoreApp.Blazor.Server.UI.Providers
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService localStorage;
        private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler;

        public ApiAuthenticationStateProvider(ILocalStorageService localStorage)
        {
            this.localStorage = localStorage;
            jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var savedToken = await localStorage.GetItemAsync<string>("accessToken");
            if (savedToken == null)
            {
                // When there is no token to save make empty claim
                return new AuthenticationState(user);
            }

            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(savedToken);
            //if the claims is expired return empty claim
            if(tokenContent.ValidTo < DateTime.Now)
            {
                return new AuthenticationState(user);
            }

            var claims = await GetClaims(); 

            //Create claim principle
            user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

            return new AuthenticationState(user);
        }

        public async Task LoggedIn()
        {
            var claims = await GetClaims();
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authstate = Task.FromResult(new AuthenticationState(user));
            NotifyAuthenticationStateChanged(authstate);
        }

        public async Task LoggedOut()
        {
            await localStorage.RemoveItemAsync("accessToken");
            var nobody = new ClaimsPrincipal(new ClaimsIdentity());
            var authstate = Task.FromResult(new AuthenticationState(nobody));
            NotifyAuthenticationStateChanged(authstate);
        }

        private async Task<List<Claim>> GetClaims()
        {
            var savedToken = await localStorage.GetItemAsync<string>("accessToken");
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(savedToken);
            var claims = tokenContent.Claims.ToList();
            claims.Add(new Claim(ClaimTypes.Name, tokenContent.Subject));
            return claims;
            
        }
    }
}
