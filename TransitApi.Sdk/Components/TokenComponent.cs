﻿using IdentityModel.Client;
using System;
using System.Threading.Tasks;
using TransportApi.Sdk;
using TransportApi.Sdk.Interfaces;

namespace TransportApi.Sdk.Components
{
    internal sealed class TokenComponent : ITokenComponent
    {
        private TokenResponse token;
        private DateTime expiryTime;
        private string clientId;
        private string clientSecret;

        public string DefaultErrorMessage
        {
            get
            {
                return "Unable to generate access token. Please check your credentials.";
            }
        }

        public TokenComponent(string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId), "ClientId cannot be null");
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientSecret), "ClientSecret cannot be null");
            }

            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public async Task<string> GetAccessToken()
        {
            if (token != null)
            {
                if (expiryTime > DateTime.UtcNow)
                {
                    return token.AccessToken;
                }
            }

            var client = new TokenClient(
                TransportApiClientConnection.IdentityStsTokenUri.ToString(),
                clientId,
                clientSecret);

            var scopes = "transportapi:all";

            try
            {
                var tokenResponse = await client.RequestClientCredentialsAsync(scopes);

                if (tokenResponse.IsError || tokenResponse.IsHttpError)
                {
                    return null;
                }

                token = tokenResponse;
                expiryTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

                return token.AccessToken;
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
