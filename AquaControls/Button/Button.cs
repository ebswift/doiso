/*

 * Known Issue - TDMS - 26/7/05:
 * During designtime, if the background of the button appears black then
 * it is due to the background of the color of the control being dropped onto
 * being transparent.  For example, the backcolor of tabcontrols are transparent
 * by default.
 * 
 */

// Code modified from codeproject article: http://www.codeproject.com/cs/miscctrl/aquabutton.asp?df=100&forumid=4632&fr=26#xx517290xx

///	
///	Button.cs
///
///		 by Dave Peckham
///		 August 2002
///		 Irvine, California
///	
///	A button that emulates the Apple Aqua user interface guidelines.
///	This button grows horizontally to accomodate its label. Button
///	height is fixed.
///	

using System;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace Wildgrape.Aqua.Controls
{
	[Description( "Aqua Button Control" )]
	[Designer(typeof (Wildgrape.Aqua.Controls.ButtonDesigner))]
	public class Button : System.Windows.Forms.Button 
	{
		#region Class Constants

		protected static int ButtonDefaultWidth = 80;
		protected static int ButtonMinWidth = 34;

		// Set this to the height of your source bitmaps
		protected static int ButtonHeight = 30;

		// If your source bitmaps have shadows, set this 
		// to the shadow size so DrawText can position the 
		// label appears centered on the label
		protected static int ButtonShadowOffset = 5;

		// These settings approximate the pulse effect
		// of buttons on Mac OS X
		protected static int PulseInterval = 70;
		protected static float PulseGammaMax = 1.8f;
		protected static float PulseGammaMin = 0.7f;
		protected static float PulseGammaShift = 0.2f;
		protected static float PulseGammaReductionThreshold = 0.2f;
		protected static float PulseGammaShiftReduction = 0.5f;

		#endregion


		#region Member Variables

		// Appearance
		protected bool	pulse = false;
		protected bool	sizeToLabel = true;
		// added by TDMS - 26/7/05 - set the default label forecolor
		protected Color labelColor = Color.Black;

		// Pulsing
		protected Timer timer;
		protected float gamma, gammaShift;

		// Mouse tracking
		protected Point ptMousePosition;
		protected bool	mousePressed;
		// added by TDMS - 26/7/05 - allows mouseover effects
		protected bool mouseEntered;

		// Images used to draw the button
		protected Image imgLeft, imgFill, imgRight;

		// Rectangles to position images on the button face
		protected Rectangle rcLeft, rcRight;

		// Matrices for transparency transformation
		protected ImageAttributes iaDefault, iaNormal, iaPressed, iaOver, iaDisabled;
		protected ColorMatrix cmDefault, cmNormal, cmPressed, cmOver, cmDisabled;

		#endregion


		#region Constructors and Initializers

		public Button( ) 
		{
			InitializeComponent(  );
			SetStyle( ControlStyles.StandardClick, true );
			// added by TDMS - 26/7/05 - this forces control to initialise when placing onto a container
			OnCreateControl();
			// added by TDMS - 26/7/05 - default button to use hand for cursor
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			//this.Height = ButtonHeight;
			//this.Height = ButtonHeight;
			//this.Width = ButtonDefaultWidth;
		}

		private void InitializeComponent( )
		{
		}

		#endregion


		#region Properties

		[Description( "Determines whether the button pulses. Note that only the default button can pulse." )]
		[Category( "Appearance" )]
		[DefaultValue( false )]
		public bool Pulse
		{
			get { return pulse; }
			set { 
				pulse = value;
				this.Refresh();
			}
		}

		[Description( "Determines whether the button should automatically size to fit the label" )]
		[Category( "Layout" )]
		[DefaultValue( true )]
		public bool SizeToLabel
		{
			get { return sizeToLabel; }
			set 
			{ 
				sizeToLabel = value;
				OnTextChanged( EventArgs.Empty ); 
			}
		}

		#endregion


		#region Property overrides

		// TDMS 11-APR-06 commented out size override, it is pointless in this implementation
		/* AquaButton has a fixed height */
//		protected override Size DefaultSize
//		{
//		    get	{return new Size( Button.ButtonDefaultWidth, 
//		             Button.ButtonHeight ); }
//		}

		/* Shadow Control.Width to make it browsable */
		[Description( "See also: SizeToLabel" )]
		[Category( "Layout" )]
		[Browsable( true )]
		public new int Width 
		{
		    get { return base.Width; }
		    set {
				if (value < ButtonMinWidth)
					base.Width = ButtonMinWidth;
				else
					base.Width = value; 
			}
		}

		/* Shadow Control.Height to make it browsable and read only */
		[Description( "Aqua buttons have a fixed height" )]
		[Category( "Layout" )]
		[Browsable( true )]
		[ReadOnly( false )]
		public new int Height {	get { return base.Height; }	}

/*
[Description( "Aqua buttons scale" )]
		[Category( "Layout" )]
		[Browsable( true )]
		[ReadOnly( false )]
		public new int Height 
		{	
			get { 
				return base.Height; 
			}
			set
			{
				this.Height = value;
				//base.Size.Height = value;
			}
		}
*/
		#endregion


		#region Method overrides

		protected override void OnCreateControl()
		{
			LoadImages();
			InitializeGraphics();
		}

		protected override void OnParentChanged(EventArgs e)
		{
			//OnCreateControl();
		}

		protected override void OnTextChanged(EventArgs e)
		{
			//if (imgRight == null)
			//    OnCreateControl();

			if (sizeToLabel) 
			{
				Graphics g = this.CreateGraphics( );
				SizeF sizeF = g.MeasureString( Text, Font );
				Width = imgLeft.Width + (int)sizeF.Width + imgRight.Width;
				g.Dispose();
			}
			Invalidate( );
			Update( );
			base.OnTextChanged( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			Graphics g = e.Graphics;
			g.Clear(Parent.BackColor);
			Draw(g);

			// added by TDMS - 8/8/05 - ensure pulsing starts if pulse property is set
			if (Pulse && (timer == null || ! timer.Enabled))
				if(! (labelColor == Color.White) && ! mousePressed )
					StartPulsing();
		}

		protected override void OnGotFocus( EventArgs e )
		{
			base.OnGotFocus( e );

			if (Pulse)
				StartPulsing();
		}
		
		protected override void OnLostFocus( EventArgs e )
		{
			base.OnLostFocus( e );

			if (Pulse)
				StopPulsing();
		}
		
		protected override void OnMouseDown( MouseEventArgs e )
		{
			//base.OnMouseDown( e ); // disabled this - see comments in the codeproject article (link at top of this code) about event firing twice

			if( e.Button == MouseButtons.Left ) 
			{
				mousePressed = true;

				ptMousePosition.X = e.X;
				ptMousePosition.Y = e.Y;

				StopPulsing( );
				Invalidate();
				Update();
			}
		}

		// added - TDMS - 26/7/05 - add mouseover effects
		protected override void OnMouseEnter(EventArgs e)
		{
			mouseEntered = true;
			StopPulsing();
			Invalidate();
			Update();
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			// Buttons receives MouseMove events when the
			// mouse enters or leaves the client area.

			base.OnMouseMove( e );

			// commented out default code - TDMS - 26/7/05 - allow mouseover effects to work properly

			//if (mouseEntered)
			//{
				//ptMousePosition.X = e.X;
				//ptMousePosition.Y = e.Y;
			//}

			//if( mousePressed && ( e.Button & MouseButtons.Left ) != 0 )
			//{
			//} 
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			if( mousePressed ) 
			{
				mousePressed = false;

				StartPulsing( );

				Invalidate();
				Update( );
			}
		}

		protected override void OnKeyPress( KeyPressEventArgs e )
		{
			base.OnKeyPress( e );

			if( mousePressed && e.KeyChar == '\x001B' )  // Escape
			{
				mousePressed = false;

				StartPulsing( );

				Invalidate();
				Update( );
			}
		}

		#endregion


		#region Implementation

		protected virtual void LoadImages ()
		{
			imgLeft = new Bitmap( GetType(), "Button.left.png" );
			imgRight = new Bitmap( GetType(), "Button.right.png" );
			imgFill = new Bitmap( GetType(), "Button.fill.png" );
		}

		protected virtual void InitializeGraphics ()
		{
			// Rectangles for placing images relative to the client rectangle
			rcLeft = new Rectangle( 0, 0, imgLeft.Width, imgLeft.Height );
			rcRight = new Rectangle( 0, 0, imgRight.Width, imgRight.Height );

			// Image attributes used to lighten default buttons

			cmDefault = new ColorMatrix();
			cmDefault.Matrix33 = 0.5f;  // reduce opacity by 50%

			iaDefault = new ImageAttributes();
			iaDefault.SetColorMatrix( cmDefault, ColorMatrixFlag.Default, 
				ColorAdjustType.Bitmap );
			
			// Image attributes that lighten and desaturate normal buttons
	
			cmNormal = new ColorMatrix();

			// desaturate the image
			cmNormal.Matrix00 = 1/3f;
			cmNormal.Matrix01 = 1/3f;
			cmNormal.Matrix02 = 1/3f;
			cmNormal.Matrix10 = 1/3f;
			cmNormal.Matrix11 = 1/3f;
			cmNormal.Matrix12 = 1/3f;
			cmNormal.Matrix20 = 1/3f;
			cmNormal.Matrix21 = 1/3f;
			cmNormal.Matrix22 = 1/3f;
			cmNormal.Matrix33 = 0.5f;  // reduce opacity by 50%

			iaNormal = new ImageAttributes();
			iaNormal.SetColorMatrix( cmNormal, ColorMatrixFlag.Default, 
				ColorAdjustType.Bitmap );

			// Create a darker image for the pressed state
			cmPressed = new ColorMatrix();
			cmPressed.Matrix33 = 0.5f; // reduce opacity by 50%

			iaPressed = new ImageAttributes();
			iaPressed.SetColorMatrix(cmPressed, ColorMatrixFlag.Default,
			ColorAdjustType.Bitmap);
			iaPressed.SetGamma(3f, ColorAdjustType.Bitmap);

			// added - TDMS - 26/7/05 - add mouseover effect
			// Create a lighter image for the pressed state
			cmOver = new ColorMatrix();
			cmOver.Matrix33 = 0.9f; // reduce opacity by 90%

			iaOver = new ImageAttributes();
			iaOver.SetColorMatrix(cmOver, ColorMatrixFlag.Default,
			ColorAdjustType.Bitmap);
			iaOver.SetGamma(1f, ColorAdjustType.Bitmap);

			// added - TDMS - 26/7/05 - add disabled effect
			cmDisabled = new ColorMatrix();
			cmDisabled.Matrix33 = .1f; // reduce opacity by 90%

			iaDisabled = new ImageAttributes();
			iaDisabled.SetColorMatrix(cmDisabled, ColorMatrixFlag.Default,
			ColorAdjustType.Bitmap);
			iaDisabled.SetGamma(1f, ColorAdjustType.Bitmap);
		}

		protected virtual void StartPulsing ()
		{
			//if ( Focused && Pulse && !this.DesignModeDetected( ) )
			try
			{
				if (timer.Enabled)
					return;
			}
			catch
			{
			}

			if (Pulse && !this.DesignModeDetected())
				{
				timer = new Timer( );
				timer.Interval = Button.PulseInterval;
				timer.Tick += new EventHandler( TimerOnTick );
				gamma = Button.PulseGammaMax;
				gammaShift = -Button.PulseGammaShift;
				timer.Start();
			}
		}

		protected virtual void StopPulsing ()
		{
			if ( timer != null )
			{
//				iaDefault.SetGamma( 1.0f, ColorAdjustType.Bitmap );
				// TDMS - 8/8/05 - changed iadefault to ianormal
				iaNormal.SetGamma(1.0f, ColorAdjustType.Bitmap);
				timer.Stop();
			}
		}

		protected virtual void Draw( Graphics g )
		{
			// TDMS - 27/7/05 - added height scaling
			float hvar = ((float)Height / 30.0f);
			g.ScaleTransform(1.0f, hvar);

			DrawButton(g);
			DrawText( g );
		}

		protected virtual void DrawButton( Graphics g )
		{
			// Update our destination rectangles
			rcRight.X = this.Width - imgRight.Width;

			if (!this.Enabled)
			{
				labelColor = Color.LightGray;
				DrawButtonState(g, iaDisabled); //iaDefault );
				return;
			}

			// TDMS - 26/7/05 - added labelcolor definitions for different button states
			if ( mousePressed )
			{
				if (ClientRectangle.Contains(ptMousePosition.X, ptMousePosition.Y))
				{
					ButtonShadowOffset = 3;
					labelColor = Color.White;
					DrawButtonState(g, iaPressed); //iaDefault );
				}
				else
				{
					ButtonShadowOffset = 5;
					labelColor = Color.White;
					DrawButtonState(g, iaOver);
//					DrawButtonState(g, iaNormal );
				}
			}
			else if (mouseEntered)
			{ // added - TDMS - 26/7/05 - mouseover effect
				mouseEntered = false;
				labelColor = Color.White;
				DrawButtonState(g, iaOver);
			}
			else
			{
				ButtonShadowOffset = 5;
				labelColor = Color.Black;
				DrawButtonState(g, iaNormal); //iaDefault );
			}
		}

		protected virtual void DrawButtonState (Graphics g, ImageAttributes ia)
		{
			TextureBrush tb;

			// Draw the left and right endcaps
			g.DrawImage( imgLeft, rcLeft, 0, 0, 
				imgLeft.Width, imgLeft.Height, GraphicsUnit.Pixel, ia );

			g.DrawImage( imgRight, rcRight, 0, 0, 
				imgRight.Width, imgRight.Height, GraphicsUnit.Pixel, ia );

			// Draw the middle
			tb = new TextureBrush( imgFill, 
				new Rectangle( 0, 0, imgFill.Width, imgFill.Height ), ia );
			tb.WrapMode = WrapMode.Tile;

			g.FillRectangle ( tb, imgLeft.Width, 0, 
				this.Width - (imgLeft.Width + imgRight.Width), 
				imgFill.Height);

			tb.Dispose( );
		}

		protected virtual void DrawText( Graphics g ) 
		{
			RectangleF layoutRect =
				new RectangleF(0, 0, this.Width,
					ButtonHeight - Button.ButtonShadowOffset);
			//new RectangleF(0, 0, this.Width, 
			//        this.Height - Button.ButtonShadowOffset );

			int LabelShadowOffset = 1;

			StringFormat fmt	= new StringFormat( );
			fmt.Alignment		= StringAlignment.Center;
			fmt.LineAlignment	= StringAlignment.Center;

			// Draw the shadow below the label
			layoutRect.Offset( 0, LabelShadowOffset );
			SolidBrush textShadowBrush	= new SolidBrush( Color.Gray );
			// added - TDMS - 26/7/05 - antialias rendering hint for nice text
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			g.DrawString( Text, Font, textShadowBrush, layoutRect, fmt );
			textShadowBrush.Dispose( );

			// and the label itself
			layoutRect.Offset( 0, -LabelShadowOffset );
			SolidBrush brushFiller = new SolidBrush(labelColor);
			g.DrawString(Text, Font, brushFiller, layoutRect, fmt);
			brushFiller.Dispose();
		}

		protected virtual void TimerOnTick( object obj, EventArgs e)
		{
			// set the new gamma level
				if ((gamma - Button.PulseGammaMin < Button.PulseGammaReductionThreshold ) || 
				(Button.PulseGammaMax - gamma < Button.PulseGammaReductionThreshold ))
				gamma += gammaShift * Button.PulseGammaShiftReduction;
			else
				gamma += gammaShift;

			if ( gamma <= Button.PulseGammaMin || gamma >= Button.PulseGammaMax )
				gammaShift = -gammaShift;

			iaNormal.SetGamma( gamma, ColorAdjustType.Bitmap );

			Invalidate( );
			Update( );
		}

		protected virtual bool DesignModeDetected()
		{
			// base.DesignMode always returns false, so try this workaround
			IDesignerHost host = 
				(IDesignerHost) this.GetService( typeof( IDesignerHost ) );

			return ( host != null );
		}

		#endregion
	}
}


