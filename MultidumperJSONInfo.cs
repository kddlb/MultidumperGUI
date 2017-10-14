using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MultidumperGUI.Annotations;
using Newtonsoft.Json;

namespace MultidumperGUI
{
    public class Containerinfo
    {
        [JsonProperty("copyright")]
        public string Copyright { get; set; }

        [JsonProperty("dumper")]
        public string Dumper { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("system")]
        public string System { get; set; }
    }

    public class Song

    {
        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class IndexedSong:Song

    {
        public IndexedSong(int id, Song baseSong)
        {
            Id = id;
            Author = baseSong.Author;
            Comment = baseSong.Comment;
            Name = baseSong.Name;
        }

        [JsonProperty("id")]
        public int Id { get; set; }
        


        
    }

    public class IncomingChannelInfo
    {
        [JsonProperty("channels")]
        public IList<string> Channels { get; set; }


    }

    public class ChannelInfo : INotifyPropertyChanged
    {
        private string _channelName;
        private double _maximumProgress;
        private double _currentProgress;

        public string ChannelName
        {
            get { return _channelName; }
            set { _channelName = value; }
        }

        public double MaximumProgress
        {
            get { return _maximumProgress; }
            set { _maximumProgress = value; OnPropertyChanged(nameof(MaximumProgress)); }
        }

        public double CurrentProgress
        {
            get { return _currentProgress; }
            set { _currentProgress = value; OnPropertyChanged(nameof(CurrentProgress)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MDOut
    {
      
        [JsonProperty("containerinfo")]
        public Containerinfo Containerinfo { get; set; }

        [JsonProperty("songs")]
        public IList<Song> Songs { get; set; }

        [JsonProperty("subsongCount")]
        public int SubsongCount { get; set; }
    }
}