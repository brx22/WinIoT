using SocketTestApp.Helper;
using SocketTestApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestApp.ViewModel
{
	public class MeterViewModel : NotificationBase<MeterDataObj>
	{
		#region コンストラクタ

		public MeterViewModel(MeterDataObj objData = null) : base(objData)
		{
			try {
				base.This.PropertyChanged += MeterViewModel_PropertyChanged;

			} catch (Exception ex) {

			}
		}

		#endregion //コンストラクタ

		#region プロパティ

		public String NoString
		{
			get { return This.No.ToString(); }
		}

		public Double MeterValue
		{
			get { return This.MeterValue; }
		}

		public String ValueString
		{
			get { return This.ValueString; }
		}

		#endregion //プロパティ

		#region イベントハンドラー

		private void MeterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			try {
				if (e.PropertyName == "MeterValue") {
					RaisePropertyChanged("MeterValue");
					RaisePropertyChanged("ValueString");
				}

			} catch (Exception ex) {

			}
		}

		#endregion //イベントハンドラー

		#region メソッド

		#endregion //メソッド

		#region メンバー変数

		#endregion //メンバー変数
	}
}
