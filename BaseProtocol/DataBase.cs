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

    class TextCheckBModel
    {       
        public int id { get; set; }
       
        public int idStick { get; set; }
      
        public string text { get; set; }
       
        public int isChecked { get; set; }
    }
    class DataBase : DbContext
    {
        public DbSet<UserModel> users { get; set; }
        public DbSet<StickModel> sticks { get; set; }
        public DbSet<FriendModel> friends { get; set; }
        public DbSet<TagModel> tags { get; set; }
        public DbSet<TextCheckBModel> textChecks { get; set; }

        public DataBase(string connectionStringName) : base(connectionStringName) { }

        //создание нового пользователя при регистрации - возвращает id пользователя или -1 если не удалось
        public static int CreateUser(string login, string password)
        {
            DataBase sticksyDB = new DataBase("Stiksy_DB");
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
        public static int CreateStick(int idCreator)
        {
            DataBase sticksyDB = new DataBase("Stiksy_DB");
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
        public static void DeleteStick(int idStick)
        {
            DataBase sticksyDB = new DataBase("Stiksy_DB");
            StickModel stickDel = sticksyDB.sticks.Find(idStick);
            if (stickDel != null)
            {
                sticksyDB.sticks.Remove(stickDel);
                sticksyDB.SaveChanges();
            }
        }

        //формирование списка пользователей для добавления в стик другими пользователями
        public static List<Friend> GetFriends()
        {
            DataBase sticksyDB = new DataBase("Stiksy_DB");
            List<Friend> friends = new List<Friend>();
            foreach (UserModel user in sticksyDB.users)
            {
                friends.Add(new Friend(user.id, user.login));
            }
            return friends;
        }



        //авторизация юзера - возвращает найденного User или null
       




        //update стика

    }

}
