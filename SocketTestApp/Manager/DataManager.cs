using SocketTestApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketTestApp.Helper;
using System.Diagnostics;
using SocketTestApp.Data;

namespace SocketTestApp.Manager
{
	public class DataManager
	{
		#region コンストラクタ

		private DataManager()
		{
			try {
				for (Int32 nNo = 1; nNo <= DataDef.METER_MAX; nNo++) {
					this._lstMeterDataObj.Add(new MeterDataObj() { No = nNo });
				}

				NetworkManager.Instance.NotifyDataReceived += NetworkManager_NotifyDataReceived;

			} catch (Exception ex) {

			}
		}

		#endregion //コンストラクタ

		#region プロパティ

		public static DataManager Instance
		{
			get
			{
				if (_Instance == null) {
					_Instance = new DataManager();
				}
				return _Instance;
			}
		}

		public List<MeterDataObj> MeterDataObjList
		{
			get
			{
				return this._lstMeterDataObj;
			}
		}

		#endregion //プロパティ

		#region イベントハンドラー

		private void NetworkManager_NotifyDataReceived(object sender, DataRecievedEventArgs e)
		{
			try {
				Int32 nIndex = 0;
				Int32 nCount = this.MeterDataObjList.Count;
				foreach (String strData in e.DataList) {
					if (nCount > nIndex) {
						Debug.WriteLine(String.Format("Received Data String : {0}", strData));
						this.MeterDataObjList[nIndex].MeterValue = Convert.ToDouble(strData);
						nIndex++;
					} else {
						break;
					}
				}

			} catch (Exception ex) {
				Debug.WriteLine(String.Format("DataReceived Failure : {0}", ex.ToString()));
			}
		}

		#endregion //イベントハンドラー

		#region メソッド

		public void InitData()
		{
			foreach (MeterDataObj objData in this.MeterDataObjList) {
				objData.InitData();
			}
		}

		#endregion //メソッド

		#region メンバー変数

		private List<MeterDataObj> _lstMeterDataObj = new List<MeterDataObj>();

		private static DataManager _Instance;

		#endregion //メンバー変数
	}
}
