using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.UI
{
	public partial class Lobby : Control
	{
		private VBoxContainer _container;
		private List<PackedScene> _serverInfoRows = new List<PackedScene>();


		public override void _Ready()
		{

		}

		private void OnNewServerDiscovered()
		{
			//var newServerInfoRow = ServersInfoRow.Instantiate();
			//newServerInfoRow.FillIPInfo();
			//newServerInfoRow.FillIPlayerInfo()
			//_serverInfoRows.Add(newServerInfoRow);
		}

		private void OnServerLost()
		{ 
			
		}

	}
}
