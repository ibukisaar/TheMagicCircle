using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace 反射镜.Play {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		public bool IsBuiltInMission {
			get { return (bool)GetValue(IsBuiltInMissionProperty); }
			set { SetValue(IsBuiltInMissionProperty, value); }
		}

		public static readonly DependencyProperty IsBuiltInMissionProperty =
			DependencyProperty.Register("IsBuiltInMission", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

		public int MissionId {
			get { return (int)GetValue(MissionIdProperty); }
			set { SetValue(MissionIdProperty, value); }
		}

		public static readonly DependencyProperty MissionIdProperty =
			DependencyProperty.Register("MissionId", typeof(int), typeof(MainWindow), new PropertyMetadata(0, OnMissionIdChanged));

		private static void OnMissionIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is MainWindow @this) {
				var oldId = (int)e.OldValue;
				var newId = (int)e.NewValue;

				@this.SaveToCache(oldId);

				if (@this.missions != null && newId > 0 && newId <= @this.missions.Length) {
					@this.SetValue(MissionNamePropertyKey, @this.missions[newId - 1].Name);
					@this.SetValue(MissionDescriptionPropertyKey, @this.missions[newId - 1].Description);
					@this.SetValue(MissionCompleteDescriptionPropertyKey, @this.missions[newId - 1].CompleteDescription);
					@this.SetValue(IsMissionCompletePropertyKey, @this.missions[newId - 1].IsMissionComplete);
					if (@this.missions[newId - 1].UserMapData != null) {
						@this.gameView.DecodeMap(@this.missions[newId - 1].UserMapData);
					} else {
						@this.gameView.DecodeMap(@this.missions[newId - 1].InitMapData);
					}
					@this.prevMissionButton.IsEnabled = newId != 1;
					@this.nextMissionButton.IsEnabled = newId != @this.EnabledMissionCount;
				} else {
					@this.ClearValue(MissionNamePropertyKey);
					@this.ClearValue(MissionDescriptionPropertyKey);
					@this.ClearValue(MissionCompleteDescriptionPropertyKey);
					@this.ClearValue(IsMissionCompletePropertyKey);
					@this.prevMissionButton.IsEnabled = false;
					@this.nextMissionButton.IsEnabled = false;
				}
			}
		}

		public string MissionName {
			get { return (string)GetValue(MissionNameProperty); }
			protected set { SetValue(MissionNamePropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey MissionNamePropertyKey =
			DependencyProperty.RegisterReadOnly("MissionName", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

		public static readonly DependencyProperty MissionNameProperty = MissionNamePropertyKey.DependencyProperty;

		public string MissionDescription {
			get { return (string)GetValue(MissionDescriptionProperty); }
			protected set { SetValue(MissionDescriptionPropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey MissionDescriptionPropertyKey =
			DependencyProperty.RegisterReadOnly("MissionDescription", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

		public static readonly DependencyProperty MissionDescriptionProperty = MissionDescriptionPropertyKey.DependencyProperty;

		public string MissionCompleteDescription {
			get { return (string)GetValue(MissionCompleteDescriptionProperty); }
			protected set { SetValue(MissionCompleteDescriptionPropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey MissionCompleteDescriptionPropertyKey =
			DependencyProperty.RegisterReadOnly("MissionCompleteDescription", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

		public static readonly DependencyProperty MissionCompleteDescriptionProperty = MissionCompleteDescriptionPropertyKey.DependencyProperty;

		public int TotalMissionCount {
			get { return (int)GetValue(TotalMissionCountProperty); }
			protected set { SetValue(TotalMissionCountPropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey TotalMissionCountPropertyKey =
			DependencyProperty.RegisterReadOnly("TotalMissionCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

		public static readonly DependencyProperty TotalMissionCountProperty = TotalMissionCountPropertyKey.DependencyProperty;

		public int EnabledMissionCount {
			get { return (int)GetValue(EnabledMissionCountProperty); }
			protected set { SetValue(EnabledMissionCountPropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey EnabledMissionCountPropertyKey =
			DependencyProperty.RegisterReadOnly("EnabledMissionCount", typeof(int), typeof(MainWindow), new PropertyMetadata(1, OnEnabledMissionCountChanged));
		
		public static readonly DependencyProperty EnabledMissionCountProperty = EnabledMissionCountPropertyKey.DependencyProperty;

		private static void OnEnabledMissionCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is MainWindow @this) {
				@this.nextMissionButton.IsEnabled = @this.MissionId < (int)e.NewValue;
			}
		}

		public bool IsMissionComplete {
			get { return (bool)GetValue(IsMissionCompleteProperty); }
			protected set { SetValue(IsMissionCompletePropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey IsMissionCompletePropertyKey =
			DependencyProperty.RegisterReadOnly("IsMissionComplete", typeof(bool), typeof(MainWindow), new PropertyMetadata(false, OnMissionCompleteChanged));

		public static readonly DependencyProperty IsMissionCompleteProperty = IsMissionCompletePropertyKey.DependencyProperty;

		private static void OnMissionCompleteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is MainWindow @this && (bool)e.NewValue) {
				if (@this.MissionId > 0 && @this.missions != null && @this.missions[@this.MissionId - 1].IsMissionComplete == false) {
					@this.missions[@this.MissionId - 1].IsMissionComplete = true;
					@this.EnabledMissionCount = Math.Min(@this.EnabledMissionCount + @this.missions[@this.MissionId - 1].EnabledMissionCount, @this.TotalMissionCount);
				}
			}
		}



		private class MissionInfo {
			public int Id { get; set; }
			public string Name { get; set; }
			public string Description { get; set; }
			public string CompleteDescription { get; set; }
			public int EnabledMissionCount { get; set; } = 1;
			public bool IsMissionComplete { get; set; }
			public byte[] InitMapData { get; set; }
			public byte[] UserMapData { get; set; }
		}

		private MissionInfo[] missions = null;

		public void LoadMissions(string json) {
			var root = JToken.Parse(json) as JArray;
			missions = new MissionInfo[root.Count];
			for (int i = 0; i < missions.Length; i++) {
				var jsonObj = root[i] as JObject;
				missions[i] = new MissionInfo {
					Id = i + 1,
					InitMapData = Convert.FromBase64String(jsonObj.Value<string>("InitData")),
				};
				if (jsonObj.ContainsKey("Name")) {
					missions[i].Name = jsonObj.Value<string>("Name");
				}
				if (jsonObj.ContainsKey("Description")) {
					missions[i].Description = jsonObj.Value<string>("Description");
				}
				if (jsonObj.ContainsKey("CompleteDescription")) {
					missions[i].CompleteDescription = jsonObj.Value<string>("CompleteDescription");
				}
				if (jsonObj.ContainsKey("UserData")) {
					missions[i].UserMapData = Convert.FromBase64String(jsonObj.Value<string>("UserData"));
				}
				if (jsonObj.ContainsKey("EnabledMissionCount")) {
					missions[i].EnabledMissionCount = jsonObj.Value<int>("EnabledMissionCount");
				}
				if (jsonObj.ContainsKey("MissionComplete")) {
					missions[i].IsMissionComplete = jsonObj.Value<bool>("MissionComplete");
				}
			}
			TotalMissionCount = missions.Length;
			if (missions.Length > 0) {
				MissionId = 1;
			}
			for (int i = 0; i < missions.Length; i++) {
				if (missions[i].IsMissionComplete) {
					EnabledMissionCount = Math.Min(EnabledMissionCount + missions[i].EnabledMissionCount, TotalMissionCount);
				}
			}
		}

		public void NextMission() {
			if (MissionId < EnabledMissionCount) {
				++MissionId;
			}
		}

		public void PrevMission() {
			if (MissionId > 1) {
				--MissionId;
			}
		}

		public void SaveToCache(int missionId) {
			if (missions != null && missionId > 0 && missionId <= missions.Length) {
				missions[missionId - 1].UserMapData = gameView.EncodeMap();
			}
		}

		public string SaveToJson() {
			if (missions != null) {
				var stringWriter = new StringWriter();
				using (var writer = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented, Indentation = 4, IndentChar = ' ' }) {
					void WriteKeyValue(string prop, object value) {
						writer.WritePropertyName(prop);
						writer.WriteValue(value);
					}

					writer.WriteStartArray();
					for (int i = 0; i < missions.Length; i++) {
						writer.WriteStartObject();
						if (!string.IsNullOrWhiteSpace(missions[i].Name)) {
							WriteKeyValue("Name", missions[i].Name);
						}
						if (!string.IsNullOrWhiteSpace(missions[i].Description)) {
							WriteKeyValue("Description", missions[i].Description);
						}
						if (!string.IsNullOrWhiteSpace(missions[i].CompleteDescription)) {
							WriteKeyValue("CompleteDescription", missions[i].CompleteDescription);
						}
						WriteKeyValue("InitData", Convert.ToBase64String(missions[i].InitMapData));
						if (missions[i].UserMapData != null) {
							WriteKeyValue("UserData", Convert.ToBase64String(missions[i].UserMapData));
						}
						if (missions[i].EnabledMissionCount > 1) {
							WriteKeyValue("EnabledMissionCount", missions[i].EnabledMissionCount);
						}
						if (missions[i].IsMissionComplete) {
							WriteKeyValue("MissionComplete", true);
						}
						writer.WriteEndObject();
					}
					writer.WriteEndArray();
					writer.Flush();
				}
				return stringWriter.GetStringBuilder().ToString();
			}
			return null;
		}

		private void main_MissionComplete(object sender, RoutedEventArgs e) {
			IsMissionComplete = true;
		}

		private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (IsBuiltInMission) {
				SaveToCache(MissionId);
				File.WriteAllText(App.ConfigFile, SaveToJson());
			}
		}

		public void LoadUserMap(string base64Data) {
			IsBuiltInMission = false;
			gameView.DecodeMap(Convert.FromBase64String(base64Data));
		}
	}
}
