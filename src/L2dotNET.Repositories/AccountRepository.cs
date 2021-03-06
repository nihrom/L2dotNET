﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using L2dotNET.DataContracts;
using L2dotNET.Logging.Abstraction;
using L2dotNET.Repositories.Contracts;
using MySql.Data.MySqlClient;

namespace L2dotNET.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        internal IDbConnection Db;

        public AccountRepository()
        {
            Db = new MySqlConnection("Server=127.0.0.1;Database=l2dotnet;Uid=l2dotnet;Pwd=l2dotnet;SslMode=none;");
        }

        public async Task<AccountContract> GetAccountByLogin(string login)
        {
            try
            {
                return (await Db.QueryAsync<AccountContract>("select Login,Password,LastActive,access_level as AccessLevel,LastServer from accounts where login=@login", new
                {
                    login = login
                })).FirstOrDefault();
            }
            catch (MySqlException ex)
            {
                Log.Error($"Method: {nameof(GetAccountByLogin)}. Message: '{ex.Message}' (Error Number: '{ex.Number}')");
                return null;
            }
        }

        public async Task<AccountContract> CreateAccount(string login, string password)
        {
            try
            {
                await Db.ExecuteAsync("insert into accounts (Login,Password,LastActive,access_level,LastServer) Values (@login,@pass,@lastactive,@access,@lastServer)", new
                {
                    login = login,
                    pass = password,
                    lastactive = DateTime.Now.Ticks,
                    access = 0,
                    lastServer = 1
                }); //to be edited

                var accContract = new AccountContract
                {
                    Login = login,
                    Password = password,
                    LastActive = DateTime.Now.Ticks,
                    AccessLevel = 0,
                    LastServer = 1
                };

                return accContract;
            }
            catch (MySqlException ex)
            {
                Log.Error($"Method: {nameof(CreateAccount)}. Message: '{ex.Message}' (Error Number: '{ex.Number}')");
                return null;
            }
        }

        public async Task<bool> CheckIfAccountIsCorrect(string login, string password)
        {
            try
            {
                return (await Db.QueryAsync("select distinct 1 from accounts where login=@login AND password=@pass", new
                {
                    login = login,
                    pass = password
                })).Any();
            }
            catch (MySqlException ex)
            {
                Log.Error($"Method: {nameof(CheckIfAccountIsCorrect)}. Message: '{ex.Message}' (Error Number: '{ex.Number}')");
                return false;
            }
        }

        public async Task<List<int>> GetPlayerIdsListByAccountName(string login)
        {
            try
            {
                return (await Db.QueryAsync<int>("select obj_Id from characters where account_name=@acc", new
                {
                    acc = login
                })).ToList();
            }
            catch (MySqlException ex)
            {
                Log.Error($"Method: {nameof(GetPlayerIdsListByAccountName)}. Message: '{ex.Message}' (Error Number: '{ex.Number}')");
                return new List<int>();
            }
        }
    }
}