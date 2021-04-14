using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using SticksyProtocol;
using System.Drawing;

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
            this.color = KnownColor.White.ToString();
        }
    }

    class TagModel
    {
        public int id { get; set; }

        public int idStick { get; set; }

        public string tag { get; set; }
    }

    class TextCheckModel
    {       
        public int id { get; set; }
       
        public int idStick { get; set; }
      
        public string text { get; set; }
       
        public bool isChecked { get; set; }
    }
    class DataBase : DbContext
    {
        public DbSet<UserModel> users { get; set; }
        public DbSet<StickModel> sticks { get; set; }
        public DbSet<FriendModel> friends { get; set; }
        public DbSet<TagModel> tags { get; set; }
        public DbSet<TextCheckModel> textChecks { get; set; }

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
        public  User GetUserFromLoginPassword(string login, string password)
        {
            
            UserModel userModel = (from u in sticksyDB.users
                                   where u.login == login && u.password == password
                                   select u).FirstOrDefault();
            if (userModel == null) return null;

            User user = new User(userModel.id, userModel.login, userModel.password);

            List<Stick> sticks = new List<Stick>();
            List<StickModel> stickModels = (from s in sticksyDB.sticks
                                            where s.idCreator == user.id
                                            select s).ToList();

            foreach (StickModel stickModel in stickModels)
            {
                Stick stick = new Stick(stickModel.id, user.id);
                stick.title = stickModel.title;
                stick.date = stickModel.date;
                stick.color = stickModel.color;

                List<string> tagsStick = new List<string>();
                List<TagModel> tagModels = (from t in sticksyDB.tags
                                            where t.idStick == stick.id
                                            select t).ToList();
                foreach (TagModel tagModel in tagModels)
                {
                    tagsStick.Add(tagModel.tag);
                }
                stick.tags = tagsStick;

                List<Friend> friendsStick = new List<Friend>();
                List<FriendModel> friendModels = (from f in sticksyDB.friends
                                                  where f.idStick == stick.id
                                                  select f).ToList();
                foreach (FriendModel friendModel in friendModels)
                {                    
                    try
                    {
                        string loginFriend = (from u in sticksyDB.users
                                              where u.id == friendModel.id
                                              select u.login).First();
                        friendsStick.Add(new Friend(friendModel.id, loginFriend));
                    }
                    catch { continue; }   
                }
                stick.Visiters = friendsStick;

                List<TextCheck> contentStick = new List<TextCheck>();
                List<TextCheckModel> textCheckModels = (from t in sticksyDB.textChecks
                                                        where t.idStick == stick.id
                                                        select t).ToList();
                foreach (TextCheckModel textCheckModel in textCheckModels)
                {
                    TextCheck textCheck = new TextCheck() { id = textCheckModel.id, text = textCheckModel.text, isChecked = textCheckModel.isChecked };
                    contentStick.Add(textCheck);
                }
                stick.content = contentStick;
                sticks.Add(stick);
            }
            return user;
        }





        //update стика

    }

}
