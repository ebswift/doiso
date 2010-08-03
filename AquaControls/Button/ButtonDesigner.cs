//
// AquaButtonDesigner.cs  by David Peckham
///
///		 by Dave Peckham
///		 Irvine, California
///		 August 20, 2002
///	
// Remove a few properties that our button doesn't support (yet).
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;


namespace Wildgrape.Aqua.Controls
{
	public class ButtonDesigner : System.Windows.Forms.Design.ControlDesigner {
	
		public ButtonDesigner()
		{
		}

		//Overrides

		/// <summary>
		/// Remove Button and Control properties that are 
		/// not supported by the Aqua Button
		/// </summary>
		/// <param name="Properties"></param>
		protected override void PostFilterProperties( IDictionary Properties )
		{
			Properties.Remove( "AllowDrop" );
			Properties.Remove( "BackColor" );
			Properties.Remove( "BackgroundImage" );
			Properties.Remove( "ContextMenu" );
			Properties.Remove( "FlatStyle" );
			Properties.Remove( "ForeColor" );
			Properties.Remove( "Image" );
			Properties.Remove( "ImageAlign" );
			Properties.Remove( "ImageIndex" );
			Properties.Remove( "ImageList" );
			//Properties.Remove( "Size" );
			Properties.Remove( "TextAlign" );
		}
	}
}
