using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Proxy.Prop;

namespace UnityBase.Objects
{
	public interface IPropertyBoundComponent
	{
		void Setup(PropertySetManager container);
		void Shutdown();
	}
	public interface IContextDefiningPropertyBoundComponent
	{}

	public interface IPropertyBoundComponentAlternatePath
	{
		string ComponentId { get; }
	}
}
