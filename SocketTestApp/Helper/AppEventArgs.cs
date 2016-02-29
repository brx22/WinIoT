using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestApp.Helper
{
	public class DataRecievedEventArgs : EventArgs
	{
		public DataRecievedEventArgs(List<String> lstData)
		{
			this.DataList = lstData;
		}

		public List<String> DataList
		{
			get;
			private set;
		}
	}

	public class StringEventArgs : EventArgs
	{
		public StringEventArgs(String strText)
		{
			this.Text = strText;
		}

		public String Text
		{
			get;
			private set;
		}
	}
}
