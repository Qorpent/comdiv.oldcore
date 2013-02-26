using System;
using Comdiv.Model.Interfaces;

namespace Comdiv.Security {
    public interface IUserRecordRepository:IWithIdx {
        IUserRecord[] GetAll();
        IUserRecord[] Search(string loginmask);
        IUserRecord Get(string login);
    }
}