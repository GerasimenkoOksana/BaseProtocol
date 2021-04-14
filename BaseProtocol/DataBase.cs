﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using SticksyProtocol;


namespace BaseProtocol
{
    class UserModel
    {
        public int id { get; set; }        
        public string login { get; set; }       
        public string password { get; set; }
        public UserModel(string login, string password)
        {
            this.login = login;
            this.password = password;
        }
    }

    class FriendModel
    {
        public int id { get; set; }  
        public int idUser { get; set; }     
        public int idStick { get; set; }
    }

    class StickModel
    {
        public int id { get; set; }        
        public string title { get; set; }
       
        public int idCreator { get; set; }
       
        public DateTime date { get; set; }
       
        public string color { get; set; }

        public StickModel(int idCreator)
        {
            this.title = "";
            this.idCreator = idCreator;
            this.date = DateTime.Now;
            this.color = "White";
        }
    }

    class TagModel
    {
        public int id { get; set; }

        public int idStick { get; set; }

        public string tag { get; set; }
        public TagModel(int idStick, string text)
        {
            this.idStick = idStick;
            this.tag = text;
        }
    }

    class CheckboxContentModel
    {       
        public int id { get; set; }
       
        public int idStick { get; set; }
      
        public string text { get; set; }
       
        public bool isChecked { get; set; }
    }

    class TextContentModel
    {
        public int id { get; set; }

        public int idStick { get; set; }

        public string text { get; set; }      
    }
    class DataBase : DbContext
    {
        public DbSet<UserModel> users { get; set; }
        public DbSet<StickModel> sticks { get; set; }
        public DbSet<FriendModel> friends { get; set; }
        public DbSet<TagModel> tags { get; set; }
        public DbSet<CheckboxContentModel> checkboxContent { get; set; }
        public DbSet<TextContentModel> textContent { get; set; }

        public DataBase(string connectionStringName) : base(connectionStringName) { }
    }
    class StiksyDataBase
    {
        private DataBase sticksyDB;
        public StiksyDataBase()
        {
            sticksyDB = new DataBase("Stiksy_DB");
        }   

        //создание нового пользователя при регистрации - возвращает id пользователя или -1 если не удалось
        public int CreateUser(string login, string password)
        {
            
            UserModel userBase = (from u in sticksyDB.users
                                  where u.login == login
                                  select u).FirstOrDefault();
            if (userBase != null) return -1; //если найден пользователь с таким же логином
           
            sticksyDB.users.Add(new UserModel(login, password));
            sticksyDB.SaveChanges();
            UserModel userNew = (from u in sticksyDB.users
                                 where u.login == login
                                 select u).FirstOrDefault();
            if (userNew == null) return -1;  //проверка, произошло ли добавление в базу 
            else return userNew.id;
        }

        //создание нового стика у существующего пользователя - возвращает id стика или -1 если не удалось
        public  int CreateStick(int idCreator)
        {            
            UserModel  creatorStick = (from u in sticksyDB.users
                                        where u.id == idCreator
                                        select u).FirstOrDefault();
            if (creatorStick == null) return -1;   //нет в базе пользователя с указанным idCreator
            else
            {
                sticksyDB.sticks.Add(new StickModel(idCreator));
                sticksyDB.SaveChanges();
                StickModel stickNew = (from s in sticksyDB.sticks
                                     where s.idCreator == idCreator
                                       select s).FirstOrDefault();
                if (stickNew == null) return -1;  //проверка, произошло ли добавление в базу 
                else return stickNew.id;             
            }              
        }

        
        //удаление стика по id
        public  void DeleteStick(int idStick)
        {            
            StickModel stickDel = sticksyDB.sticks.Find(idStick);
            if (stickDel != null)
            {
                sticksyDB.sticks.Remove(stickDel);
                sticksyDB.SaveChanges();
            }

            var tagsDel = from t in sticksyDB.tags
                          where t.idStick == idStick
                          select t;            
            foreach (var tag in tagsDel)
            {
                sticksyDB.tags.Remove(tag);
            }           

            var friendsDel = from f in sticksyDB.friends
                             where f.idStick == idStick
                             select f;
            foreach (var friend in friendsDel)
            {
                sticksyDB.friends.Remove(friend);
            }

            var checkboxContentDel = from c in sticksyDB.checkboxContent
                                     where c.idStick == idStick
                                     select c;
            foreach (var checkbox in checkboxContentDel)
            {
                sticksyDB.checkboxContent.Remove(checkbox);
            }

            var textContentDel = from t in sticksyDB.textContent
                                 where t.idStick == idStick
                                 select t;
            foreach (var text in textContentDel)
            {
                sticksyDB.textContent.Remove(text);
            }

            sticksyDB.SaveChanges();
        }

        //формирование списка пользователей для добавления в стик другими пользователями
        public  List<Friend> GetFriends()
        {           
            List<Friend> friends = new List<Friend>();
            foreach (UserModel user in sticksyDB.users)
            {
                friends.Add(new Friend(user.id, user.login));
            }
            return friends;
        }


        //авторизация юзера - возвращает найденного User или null
        public  User GetUserByLoginPassword(string login, string password)
        {
            
            UserModel userModel = (from u in sticksyDB.users
                                   where u.login == login && u.password == password
                                   select u).FirstOrDefault();
            if (userModel == null) return null;

            User user = new User(userModel.id, userModel.login);

            List<Stick> sticks = new List<Stick>();
            var stickModels = from s in sticksyDB.sticks
                                            where s.idCreator == user.id
                                            select s;

            foreach (StickModel stickModel in stickModels)
            {
                Stick stick = new Stick(stickModel.id, user.id);
                stick.title = stickModel.title;
                stick.date = stickModel.date;
                stick.color = stickModel.color;                               
                stick.tags = GetTagsByIdStick(stick.id);                
                stick.visiters = GetFriendsByIdStick(stick.id);    
                stick.content = GetContentByIdStick(stick.id);
                sticks.Add(stick);
            }
            return user;
        }
                
        private List<string> GetTagsByIdStick(int idStick)
        {
            List<string> tagsStick = new List<string>();
            var tagModels = from t in sticksyDB.tags
                            where t.idStick == idStick
                            select t;
            foreach (TagModel tagModel in tagModels)
            {
                tagsStick.Add(tagModel.tag);
            }
            return tagsStick;
        }

        private List<Friend> GetFriendsByIdStick(int idStick)
        {
            List<Friend> friendsStick = new List<Friend>();
            var friendModels = from f in sticksyDB.friends
                               where f.idStick == idStick
                               join u in sticksyDB.users on f.idUser equals u.id
                               select new { f.idUser, u.login };
            foreach (var friend in friendModels)
            {
                friendsStick.Add(new Friend(friend.idUser, friend.login));
            }
            return friendsStick;
        }

        private List<IStickContent> GetContentByIdStick(int idStick)
        {
            List<IStickContent> contentStick = new List<IStickContent>();
            var textModels = from t in sticksyDB.textContent
                             where t.idStick == idStick
                             select t;
            foreach (TextContentModel text in textModels)
            {
                TextContent textContent = new TextContent(text.id, text.text);
                contentStick.Add(textContent);
            }
            var checkboxModels = from c in sticksyDB.checkboxContent
                                 where c.idStick == idStick
                                 select c;
            foreach (CheckboxContentModel check in checkboxModels)
            {
                CheckboxContent checkbox = new CheckboxContent(check.id, check.text, check.isChecked);
                contentStick.Add(checkbox);
            }           
            return contentStick;
        }

        //update стика - возвращает bool (успешно ли обновление)
        public bool UpdateStick(Stick stick)
        {
            UserModel userModel = (from u in sticksyDB.users
                                    where u.id == stick.idCreator
                                    select u).FirstOrDefault();
            if (userModel == null)
            {
                DeleteStick(stick.id);
                sticksyDB.SaveChanges();
                return false;
            }
            StickModel stickBase = (from s in sticksyDB.sticks
                                    where s.id == stick.id
                                    select s).FirstOrDefault();
            if (stickBase == null || stickBase.idCreator != stick.idCreator)
            {
                return false;
            }
            stickBase.title = stick.title;
            stickBase.date = stick.date;
            stickBase.color = stick.color;
            UpdateTags(stick.id, stick.tags);
            UpdateFriends(stick.id, stick.visiters);
            UpdateContent(stick.id, stick.content);
            return true;
        }

        private void UpdateTags(int idStick, List<string> tags)
        {
            List<string> tagsBase = GetTagsByIdStick(idStick);
            foreach (string tag in tags)
            {
                if (!tagsBase.Contains(tag))
                    sticksyDB.tags.Add(new TagModel(idStick, tag));
            }
            foreach (string tagBase in tagsBase)
            {
                if (!tags.Contains(tagBase))
                {
                    TagModel tm = (from t in sticksyDB.tags
                                  where t.idStick == idStick && t.tag == tagBase
                                  select t).FirstOrDefault();
                    try
                    {
                        sticksyDB.tags.Remove(tm);
                    }
                    catch { }
                }
            }
            sticksyDB.SaveChanges();
        }

        private void UpdateFriends (int idStick, List<Friend> friends)
        {
            //To Do
        }

        private void UpdateContent(int idStick, List<IStickContent> content)
        {
            //To Do
        }
    }

}
