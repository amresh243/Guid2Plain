using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Guid2Plain
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    public MainWindow()
    {
      this.DataContext = this;
      InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      Input = InputPlaceHolder;
    }

    /// <summary> Property to access input outside </summary>
    public string Input
    {
      get { return _Input; }
      set
      {
        if (value != _Input)
        {
          _Input = value;
          RaisePropertyChanged("Input");
        }
      }
    }

    /// <summary> Property to access output outside </summary>
    public string Output
    {
      get { return _Output; }
      set
      {
        if (value != _Output)
        {
          _Output = value;
          RaisePropertyChanged("Output");
        }
      }
    }

    /// <summary> Binding notification handler </summary>
    /// <param name="propertyName"> Name of property against which change triggered </param>
    protected void RaisePropertyChanged(string propertyName) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool IsValidInput(string data)
    {
      if (data.Length == 32 || data.Length == 36 || data.Length == 38)
        return true;

      return false;
    }

    private void Convert(object sender, RoutedEventArgs e)
    {
      string strInput = _Input;
      if (string.IsNullOrEmpty(strInput))
        return;

      strInput = strInput.Trim();
      if(!IsValidInput(strInput))
      {
        DisplayMessage("Invalid input.", MessageType.Error);
        return;
      }

      if(IsPlainText(strInput))
      {
        strInput = strInput.Insert(0, "{").Insert(9, "-").Insert(14, "-").Insert(19, "-").Insert(24, "-");
        strInput += "}";
        Output = strInput;
        DisplayMessage("Text converted to guid and copied to clipboard.");
        Clipboard.Clear();
        Clipboard.SetText(strInput);
        return;
      }

      strInput = strInput.Replace("{", "").Replace("}", "").Replace("-", "");
      Output = strInput;
      DisplayMessage("Guid converted to text and copied to clipboard.");
      Clipboard.Clear();
      Clipboard.SetText(strInput);
    }

    private bool IsPlainText(string data)
    {
      string inputData = data.Trim();
      if (inputData.Length != 32)
      {
        DisplayMessage("Invalid input.", MessageType.Error);
        return false;
      }

      return true;
    }

    private void InputGotFocus(object sender, RoutedEventArgs e)
    {
      Input = "";
      txtGuidInput.Foreground = Brushes.Blue;
      string input = Clipboard.GetText();
      if(!IsValidInput(input))
      {
        Input = "";
        DisplayMessage("Valid input not found in clipboard.", MessageType.Warning);
        return;
      }
      else if (!string.IsNullOrEmpty(input))
      {
        Input = input;
        Convert(null, null);
        txtGuidOutput.Focus();
      }
    }

    private void DisplayMessage(string message, MessageType msgType=MessageType.Info)
    {
      lbIcon.Visibility = Visibility.Visible;
      switch(msgType)
      {
        case MessageType.Info:
          lbStatus.Foreground = Brushes.White;
          lbStatus.Content = message;
          lbIcon.Source = ImageSourceForBitmap(Properties.Resources.info);
          break;
        case MessageType.Warning:
          lbStatus.Foreground = Brushes.Orange;
          lbStatus.Content = string.Format("Warning! {0}", message);
          lbIcon.Source = ImageSourceForBitmap(Properties.Resources.warning);
          break;
        case MessageType.Error:
          lbStatus.Foreground = Brushes.Tomato;
          lbStatus.Content = string.Format("Error! {0}", message);
          lbIcon.Source = ImageSourceForBitmap(Properties.Resources.error);
          break;
      }
    }

    public ImageSource ImageSourceForBitmap(System.Drawing.Bitmap bmp)
    {
      var handle = bmp.GetHbitmap();
      try
      {
        return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
      }
      finally { DeleteObject(handle); }
    }

    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject([In] IntPtr hObject);
    public event PropertyChangedEventHandler PropertyChanged;
    private enum MessageType {Error, Warning, Info};
    private readonly string InputPlaceHolder = "<Enter valid input guid or text>";
    private string _Input;
    private string _Output;
  }
}
