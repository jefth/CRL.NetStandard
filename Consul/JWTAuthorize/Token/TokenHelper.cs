﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWTAuthorize
{
    public class User
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
    public enum TokenType
    {
        AccessToken = 1,
        RefreshToken = 2
    }
    public class TokenHelper : ITokenHelper
    {
        private IOptions<JWTConfig> _options;
        public TokenHelper(IOptions<JWTConfig> options)
        {
            _options = options;
        }

        public Token CreateAccessToken(User user)
        {
            Claim[] claims = new Claim[] { new Claim(ClaimTypes.NameIdentifier, user.Code), new Claim(ClaimTypes.Name, user.Name) };

            return CreateToken(claims, TokenType.AccessToken);
        }

        public ComplexToken CreateToken(User user)
        {
            Claim[] claims = new Claim[] { new Claim(ClaimTypes.NameIdentifier, user.Code), new Claim(ClaimTypes.Name, user.Name) };

            //下面对code为001的张三添加了一个Claim，用于测试在Token中存储用户的角色信息，对应测试在FlyLolo.JWT.API的BookController的Put方法，若用不到可删除
            if (user.Code.Equals("001"))
            {
                claims = claims.Append(new Claim(ClaimTypes.Role, "TestPutBookRole")).ToArray();
            }
            
            return CreateToken(claims);
        }

        public ComplexToken CreateToken(Claim[] claims)
        {
            return new ComplexToken { AccessToken = CreateToken(claims, TokenType.AccessToken), RefreshToken = CreateToken(claims, TokenType.RefreshToken) };
        }

        /// <summary>
        /// 用于创建AccessToken和RefreshToken。
        /// 这里AccessToken和RefreshToken只是过期时间不同，【实际项目】中二者的claims内容可能会不同。
        /// 因为RefreshToken只是用于刷新AccessToken，其内容可以简单一些。
        /// 而AccessToken可能会附加一些其他的Claim。
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        private Token CreateToken(Claim[] claims, TokenType tokenType)
        {
            var now = DateTime.Now;
            var expires = now.Add(TimeSpan.FromMinutes(tokenType.Equals(TokenType.AccessToken) ? _options.Value.AccessTokenExpiresMinutes : _options.Value.RefreshTokenExpiresMinutes));
            var token = new JwtSecurityToken(
                issuer: _options.Value.Issuer,
                audience: tokenType.Equals(TokenType.AccessToken) ? _options.Value.Audience : _options.Value.RefreshTokenAudience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Secret)), SecurityAlgorithms.HmacSha256));
            return new Token { TokenContent = new JwtSecurityTokenHandler().WriteToken(token), Expires = expires };
        }

        public Token RefreshToken(ClaimsPrincipal claimsPrincipal)
        {
            var code = claimsPrincipal.Claims.FirstOrDefault(m => m.Type.Equals(ClaimTypes.NameIdentifier));
            if (null != code)
            {
                return CreateAccessToken(new User() { Name = code.Value, Code = "" });//todo
            }
            else
            {
                return null;
            }
        }
    }
}
