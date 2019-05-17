using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BackEndMS.Models
{
    public class UserInfo:INotifyPropertyChanged
    {
        public UserInfo(UserInfo user)
        {
            Id = user.Id;
            Username = user.Username;
            Nickname = user.Nickname;

            Password = user.Password;
            Email = user.Email;
            Enable = user.Enable;
            StcUser = new STCInfo()
            {
                Username = user.StcUser.Username,
                Password = user.StcUser.Password,
            };
            Role = new List<RoleType>();
            if (user.Role.Count != 0)
                Role.AddRange(user.Role);
        }
        public UserInfo()
        {
            StcUser = new STCInfo();
            Role = new List<RoleType>();
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private string _UserName;
        private string _Password;
        private string _Email;
        public string Id { get; set; }
        public string Username {
            get { return _UserName; }
            set {
                if(_UserName != value)
                {
                    _UserName = value;
                    if(null != PropertyChanged)
                        NotifyPropertyChange("Username");
                }
            }
        }
        public  string Nickname { get; set; }
        public  string Password
        {
            get { return _Password; }
            set
            {
                if (_Password != value)
                {
                    _Password = value;
                    if (null != PropertyChanged)
                        NotifyPropertyChange("Password");
                }
            }
        }
        public string Email
        {
            get { return _Email; }
            set
            {
                if (_Email != value)
                {
                    _Email = value;
                    if (null != PropertyChanged)
                        NotifyPropertyChange("Email");
                }
            }
        }
        private bool _Enable = true;
        public bool Enable { get { return _Enable; }
            set {
                if (_Enable != value)
                {
                    _Enable = value;
                    if (null != PropertyChanged)
                        NotifyPropertyChange("Enable");
                }
            }
        }
        public STCInfo StcUser { get; set; }
        public bool selected { get; set; } = false;
        public List<RoleType> Role { get; set; }
        public long LastLoginDate { get; set; }
        public string LastLoginAddress { get; set; }
        private void NotifyPropertyChange([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class STCInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoleType
    {
        Admin,
        User,
    }
}
