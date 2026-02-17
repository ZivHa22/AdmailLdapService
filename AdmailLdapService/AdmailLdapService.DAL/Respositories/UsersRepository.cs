using AdmailLdapService.DAL.DataAccess;
using AdmailLdapService.DAL.Interfaces;
using AdmailLdapService.Models;
using Microsoft.EntityFrameworkCore;


namespace AdmailLdapService.DAL.Respositories
{
    public class UsersRepository : IUsersRepository
    {


        AdmailDbContext context { get; }
        public UsersRepository(AdmailDbContext _context)
        {
            context = _context;
        }
        public void AddUsersAd(List<Domainuser> users)
        {
            try
            {
                context.AddRange(users);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public UserUpdateStatus AddUserAd(Domainuser user)
        {
            try
            {
                DateTime savedate = DateTime.Now;
                UserUpdateStatus userUpdateStatus = new UserUpdateStatus();
                if (CheckUserExsitByEmail(user.UserEmail))
                {
                    userUpdateStatus.seccussUpdate = false;
                    userUpdateStatus.errorCode = (int)UserStatuesError.EmailExist;
                    return userUpdateStatus;
                }
                else if (CheckUserExsitByUserName(user.UserName))
                {
                    Tbladministration tbladministration = context.Tbladministrations.FirstOrDefault();
                    if (tbladministration != null)
                    {
                        switch (tbladministration.TableClient)
                        {
                            case (true):
                                userUpdateStatus.seccussUpdate = false;
                                userUpdateStatus.errorCode = (int)UserStatuesError.UserNameExsit;
                                return userUpdateStatus;

                            case (false):
                                userUpdateStatus.userDuplicate = user.UserName;
                                user.UserName = user.UserName + "-" + user.UserEmail;
                                if (user.UserName.Length > 50) //TODO fix in the next version the DB nvarchar above 50
                                {
                                    user.UserName = user.UserName.Substring(0, 50);
                                }

                                context.Domainusers.Add(user);
                                context.SaveChanges();
                                userUpdateStatus.seccussUpdate = true;
                                userUpdateStatus.errorCode = (int)UserStatuesError.UserNameExsit;
                                userUpdateStatus.domainuserSaved = user;
                                UpdateLastLoadUser(savedate);
                                return userUpdateStatus;
                        }
                    }
                    return userUpdateStatus;
                }
                else
                {
                    context.Domainusers.Add(user);
                    context.SaveChanges();
                    userUpdateStatus.seccussUpdate = true;
                    userUpdateStatus.errorCode = (int)UserStatuesError.NoError;
                    userUpdateStatus.domainuserSaved = user;
                    UpdateLastLoadUser(savedate);
                    return userUpdateStatus;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public UserUpdateStatus AddGroup(Domainuser group)
        {
            try
            {
                DateTime savedate = DateTime.Now;
                UserUpdateStatus userUpdateStatus = new UserUpdateStatus();
                Domainuser userGroup = context.Domainusers.Where(g => g.UserName == group.UserName).FirstOrDefault();
                if (userGroup == null)
                {
                    context.Domainusers.Add(group);
                    context.SaveChanges();
                    userUpdateStatus.seccussUpdate = true;
                    userUpdateStatus.errorCode = (int)UserStatuesError.NoError;
                    UpdateLastLoadUser(savedate);
                    return userUpdateStatus;
                }
                else
                {
                    userUpdateStatus.seccussUpdate = false;
                    userUpdateStatus.errorCode = (int)UserStatuesError.GroupExist;
                    return userUpdateStatus;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool InsertUserGroup(List<Usersgroup> usersGroup, string UserName)
        {
            try
            {
                List<Usersgroup> usersgroupExist = context.Usersgroups.Where(ug => ug.UserName == UserName).ToList();
                if (usersgroupExist.Count > 0)
                {
                    context.Usersgroups.RemoveRange(usersgroupExist);
                    context.SaveChanges();
                }
                context.Usersgroups.AddRange(usersGroup);
                context.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteAllDomainUsers()
        {
            try
            {
                context.Domainusers.RemoveRange(context.Domainusers.ToList());
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private void UpdateLastLoadUser(DateTime lastLoad)
        {
            try
            {
                Tbladministration tbladministration = context.Tbladministrations.AsNoTracking().FirstOrDefault();
                tbladministration.LastLoad = lastLoad;
                context.ChangeTracker.Clear();
                context.Tbladministrations.Update(tbladministration);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteAllUsersGroups()
        {
            try
            {
                context.Usersgroups.RemoveRange(context.Usersgroups.ToList());
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private bool CheckUserExsitByEmail(string email)
        {
            if (context.Domainusers.Any(u => u.UserEmail == email))
            {
                return true;
            }
            else { return false; }
        }
        private bool CheckUserExsitByUserName(string userName)
        {
            if (context.Domainusers.Any(u => u.UserName == userName))
            {
                return true;
            }
            else { return false; }
        }

    }
}