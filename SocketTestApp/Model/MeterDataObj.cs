using SocketTestApp.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestApp.Model
{
	public class MeterDataObj : NotificationBase
	{
		#region コンストラクタ

		public MeterDataObj()
		{


		}

		#endregion //コンストラクタ

		#region プロパティ

		public Int32 No
		{
			get;
			set;
		}

		public Double MeterValue
		{
			get { return _MeterValue; }
			set
			{
				if (SetProperty(ref _MeterValue, value)) {
					RaisePropertyChanged(nameof(ValueString));
				}
			}
		}

		public String ValueString
		{
			get
			{
				return this.MeterValue.ToString();
			}
		}

		#endregion //プロパティ

		#region メソッド

		public void InitData()
		{
			this.MeterValue = 0;
		}

		#endregion //メソッド

		#region メンバー変数

		private Double _MeterValue;

		#endregion //メンバー変数
	}
}
