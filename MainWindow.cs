using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace GameColorWheelCreator
{
    public partial class MainWindow : Form
    {
        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            ofdImage.Filter = Utils.GetOpenImageFilter();
            sfdImage.Filter = Utils.GetSaveImageFilter();

            UpdateExportImageProperties();

            dgvColorWheelItems.AutoGenerateColumns = false;
            cbxColorWheelDirection.SelectedIndex = 0;

            UpdateBackgroundProperties(Color.Gray);
            UpdateEyedropper();
            UpdateColorWheelProperties();
            UpdateSpriteProperties();

        }

        #endregion

        #region Private Fields

        private Bitmap _exportImage;
        private Graphics _graphics;

        private int _exportImageWidth;
        private int _exportImageHeight;

        private float _spritePerimeter;
        private float _spriteOffsetX;
        private float _spriteOffsetY;
        private float _spriteRotation;
        private float _spriteScale;

        private Color _backgroundColor;
        private bool _eyedropperActive;

        private List<ColorWheelItem> _colorWheelCollection;

        private Bitmap _spriteImage;
        private bool _colorWheelClockwise;
        private float _colorWheelOutterPerimeter;
        private float _colorWheelInnerPerimeter;
        private float _colorWheelRotation;

        #endregion

        #region Private Methods

        private void ExportImage()
        {
            if (sfdImage.ShowDialog(this) != DialogResult.OK) return;

            DrawBitmap();

            var extension = Path.GetExtension(sfdImage.FileName);

            if (extension == ".BMP") 
                _exportImage.Save(sfdImage.FileName, ImageFormat.Bmp);
            else if (extension == ".JPG")
                _exportImage.Save(sfdImage.FileName, ImageFormat.Jpeg);
            else if (extension == ".GIF")
                _exportImage.Save(sfdImage.FileName, ImageFormat.Gif);
            else if (extension == ".TIF")
                _exportImage.Save(sfdImage.FileName, ImageFormat.Tiff);
            else if (extension == ".PNG")
                _exportImage.Save(sfdImage.FileName, ImageFormat.Png);
        }

        private void EditColorWheelItem(ColorWheelItem colorWheelItem, bool f)
        {
            if (colorWheelItem.CustomOccurency == 0f)
                colorWheelItem.CustomOccurency = colorWheelItem.OriginalOccurency;
            else
                colorWheelItem.CustomOccurency = 0;

            var usedPercentage = _colorWheelCollection.Sum(cwi => cwi.CustomOccurency);
            var factor = 100f / usedPercentage;

            foreach (var item in _colorWheelCollection)
                item.CustomOccurency *= factor;

            if (!f)
                UpdateGrid();
        }


        private void UpdateExportImageProperties()
        {
            _exportImageWidth = (int)nudExportImageWidth.Value;
            _exportImageHeight = (int)nudExportImageHeight.Value;
            _exportImage = new Bitmap(_exportImageWidth, _exportImageHeight);
            _graphics = Graphics.FromImage(_exportImage);
            _graphics.SmoothingMode = SmoothingMode.AntiAlias;
        }


        private void UpdateSprite()
        {
            if (ofdImage.ShowDialog(this) != DialogResult.OK) return;
            _spriteImage = new Bitmap(ofdImage.FileName);
            btnSprite.Text = ofdImage.SafeFileName;

            var colorDictionary = new Dictionary<Color, int>();

            var total = 0;
            for (int x = 0; x < _spriteImage.Width; x++)
                for (int y = 0; y < _spriteImage.Height; y++)
                {
                    var color = _spriteImage.GetPixel(x, y);
                    if (color.A != 255)
                        continue;
                    else
                        total++;

                    if (!colorDictionary.ContainsKey(color))
                        colorDictionary.Add(color, 1);
                    else
                        colorDictionary[color]++;
                }

            var colorWheelCollection = new List<ColorWheelItem>();
            foreach (var pair in colorDictionary)
            {
                var occurency = (pair.Value / (float)total) * 100f;
                var colorWheelItem = new ColorWheelItem(pair.Key, occurency);
                colorWheelCollection.Add(colorWheelItem);
            }

            _colorWheelCollection = colorWheelCollection;
            UpdateGrid();

            var spriteScaleValue = (int)(Math.Min(_exportImage.Width, _exportImage.Height) / Math.Min(_spriteImage.Width, _spriteImage.Height) * 10f);
            if (spriteScaleValue == 0)
                spriteScaleValue = 1;
            tkbSpriteScale.Value = spriteScaleValue;
            DrawBitmap();

            foreach(var colorWheelItem in _colorWheelCollection)
            {
                if (colorWheelItem.OriginalOccurency < 0.5f)
                    EditColorWheelItem(colorWheelItem, true);
            }
        }

        private void UpdateGrid()
        {
            _colorWheelCollection = (from colorWheelItem in _colorWheelCollection
                                     orderby colorWheelItem.CustomOccurency descending
                                     select colorWheelItem).ToList<ColorWheelItem>();

            dgvColorWheelItems.DataSource = null;
            dgvColorWheelItems.DataSource = _colorWheelCollection;

            DrawBitmap();
        }

        private void UpdateSpriteProperties()
        {
            _spritePerimeter = tkbSpritePerimeter.Value / 100f;
            _spriteOffsetX = tkbSpriteOffsetX.Value / 100f;
            _spriteOffsetY = tkbSpriteOffsetY.Value / 100f;
            _spriteScale = tkbSpriteScale.Value / 10f;
            _spriteRotation = tkbSpriteRotation.Value;

            lblSpritePerimeter.Text = tkbSpritePerimeter.Value.ToString() + "%";
            lblSpriteOffsetX.Text = tkbSpriteOffsetX.Value.ToString() + "%";
            lblSpriteOffsetY.Text = tkbSpriteOffsetY.Value.ToString() + "%";
            lblSpriteScale.Text = (tkbSpriteScale.Value / 10f).ToString() + " x";
            lblSpriteRotation.Text = tkbSpriteRotation.Value.ToString() + "°";

            DrawBitmap();
        }


        private void UpdateColorWheelProperties()
        {
            _colorWheelClockwise = cbxColorWheelDirection.SelectedIndex == 0;
            _colorWheelOutterPerimeter = tkbColorWheelOutterPerimeter.Value / 100f;
            _colorWheelInnerPerimeter = tkbColorWheelInnerPerimeter.Value / 100f;
            _colorWheelRotation = tkbColorWheelRotation.Value;

            lblColorWheelOutterPerimeter.Text = tkbColorWheelOutterPerimeter.Value.ToString() + "%";
            lblColorWheelInnerPerimeter.Text = tkbColorWheelInnerPerimeter.Value.ToString() + "%";
            lblColorWheelRotation.Text = tkbColorWheelRotation.Value.ToString() + "°";

            DrawBitmap();
        }


        private void UpdateBackgroundProperties(Color color)
        {
            _backgroundColor = color;

            if (tkbBackgroundRed.Value != color.R)
                tkbBackgroundRed.Value = color.R;

            if (tkbBackgroundGreen.Value != color.G)
                tkbBackgroundGreen.Value = color.G;

            if (tkbBackgroundBlue.Value != color.B)
                tkbBackgroundBlue.Value = color.B;

            lblBackgroundRed.Text = tkbBackgroundRed.Value.ToString();
            lblBackgroundGreen.Text = tkbBackgroundGreen.Value.ToString();
            lblBackgroundBlue.Text = tkbBackgroundBlue.Value.ToString();

            var colorString = color.ToHexValue();
            if (!string.Equals(txtBackgroundHexColor.Text, colorString))
                txtBackgroundHexColor.Text = colorString;
            txtBackgroundHexColor.ForeColor = SystemColors.WindowText;

            DrawBitmap();
        }

        private void UpdateEyedropper()
        {
            var activateEyedropper = ckbEyedropper.Checked;

            if (activateEyedropper != _eyedropperActive)
            {
                _eyedropperActive = activateEyedropper;
                Capture = _eyedropperActive;
            }

            if (_eyedropperActive)
            {
                var color = Eyedropper.GetPixelColor(Cursor.Position);
                UpdateBackgroundProperties(color);
            }
        }


        private void DrawBitmap()
        {
            DrawBackground(_graphics);

            if (_spriteImage != null)
            {
                DrawColorWheel(_graphics);
                DrawSpriteImage(_graphics);
            }

            _graphics.Save();
            pbxPreview.Image = _exportImage;
        }

        private void DrawBackground(Graphics graphics)
        {
            graphics.Clear(_backgroundColor);
        }

        private void DrawColorWheel(Graphics graphics)
        {
            var outterPerimeter = Math.Min(_exportImage.Width, _exportImage.Height) * _colorWheelOutterPerimeter;
            var outterLocationX = ((_exportImageWidth)  / 2f) - (outterPerimeter / 2f);
            var outterLocationY = ((_exportImageHeight) / 2f) - (outterPerimeter / 2f);

            if (_colorWheelCollection != null)
            {
                var lastOccurrency = 0f;
                if (!_colorWheelClockwise)
                    lastOccurrency = 360f;

                foreach (var colorWheelItem in _colorWheelCollection)
                {
                    var startAngle = lastOccurrency + _colorWheelRotation;
                    var sweepAngle = (colorWheelItem.CustomOccurency / 100f) * 360f;
                    if (!_colorWheelClockwise)
                        sweepAngle = -sweepAngle;

                    var colorWheelBrush = new SolidBrush(colorWheelItem.Color);
                    graphics.FillPie(colorWheelBrush, outterLocationX, outterLocationY, outterPerimeter, outterPerimeter, startAngle, sweepAngle);
                    lastOccurrency += sweepAngle;
                }
            }

            var innerPerimeter = Math.Min(_exportImage.Width, _exportImage.Height) * _colorWheelInnerPerimeter;
            var innerLocationX = ((_exportImageWidth) / 2f) - (innerPerimeter / 2f);
            var innerLocationY = ((_exportImageHeight) / 2f) - (innerPerimeter / 2f);
            var innnerBrush = new SolidBrush(_backgroundColor);

            graphics.FillPie(innnerBrush, innerLocationX, innerLocationY, innerPerimeter, innerPerimeter, 0, 360f);
        }

        private void DrawSpriteImage(Graphics graphics)
        {
            var fillPerimeter = Math.Min(_exportImage.Width, _exportImage.Height) * _spritePerimeter;
            var fillLocationX = (_exportImageWidth / 2f) - (fillPerimeter / 2);
            var fillLocationY = (_exportImageHeight / 2f) - (fillPerimeter / 2);

            var rotationX = (_spriteImage.Width / 2f * _spriteScale);
            var rotationY = (_spriteImage.Height / 2f * _spriteScale);
            var rotationPivot = new PointF(rotationX, rotationY);

            var centerTranslationX = (_exportImageWidth / 2f) - (_spriteImage.Width / 2f * _spriteScale);
            var centerTranslationY = (_exportImageHeight / 2f) - (_spriteImage.Width / 2f * _spriteScale);

            var offsetTranslationX = ((fillPerimeter / 2f) + (_spriteImage.Width / 2f * _spriteScale)) * _spriteOffsetX;
            var offsetTranslationY = ((fillPerimeter / 2f) + (_spriteImage.Height / 2f * _spriteScale)) * _spriteOffsetY;

            var transform = new Matrix();
            transform.Scale(_spriteScale, _spriteScale, MatrixOrder.Append);
            transform.RotateAt(_spriteRotation, rotationPivot, MatrixOrder.Append);
            transform.Translate(centerTranslationX , centerTranslationY, MatrixOrder.Append);
            transform.Translate(offsetTranslationX, offsetTranslationY, MatrixOrder.Append);

            var spriteBrush = new TextureBrush(_spriteImage);
            spriteBrush.Transform = transform;
            spriteBrush.WrapMode = WrapMode.Clamp;

            graphics.FillPie(spriteBrush, fillLocationX, fillLocationY, fillPerimeter, fillPerimeter, 0, 360f);
        }

        #endregion

        #region Signed Events Methods

        private void nudExportImage_ValueChanged(object sender, EventArgs e)
        {
            UpdateExportImageProperties();
        }


        private void btnSprite_Click(object sender, EventArgs e)
        {
            UpdateSprite();
        }

        private void sprite_ValueChanged(object sender, EventArgs e)
        {
            UpdateSpriteProperties();
        }


        private void tkbColors_ValueChanged(object sender, EventArgs e)
        {
            var color = Color.FromArgb(tkbBackgroundRed.Value, tkbBackgroundGreen.Value, tkbBackgroundBlue.Value);
            UpdateBackgroundProperties(color);
        }

        private void txtHexColor_TextChanged(object sender, EventArgs e)
        {
            if (!txtBackgroundHexColor.Text.StartsWith("#"))
                txtBackgroundHexColor.Text = "#" + txtBackgroundHexColor.Text.Replace("#", string.Empty);

            txtBackgroundHexColor.SelectionStart = txtBackgroundHexColor.Text.Length;
            txtBackgroundHexColor.SelectionLength = 0;

            if (txtBackgroundHexColor.Text.InvalidHexValue())
            {
                txtBackgroundHexColor.ForeColor = Color.Maroon;
                System.Media.SystemSounds.Beep.Play();
                return;
            }
            else
                txtBackgroundHexColor.ForeColor = SystemColors.WindowText;

            if (txtBackgroundHexColor.Text.Length < 7)
                return;

            if (txtBackgroundHexColor.Text.Length > 7)
                txtBackgroundHexColor.Text = txtBackgroundHexColor.Text.Substring(0, 7);

            var color = ColorTranslator.FromHtml(txtBackgroundHexColor.Text);
            UpdateBackgroundProperties(color);
        }

        private void ckbEyedropper_CheckStateChanged(object sender, EventArgs e)
        {
            UpdateEyedropper();
        }


        private void colorWheel_ValueChanged(object sender, EventArgs e)
        {
            UpdateColorWheelProperties();
        }


        private void btnExportImage_Click(object sender, EventArgs e)
        {
            ExportImage();
        }

        private void dgvColorWheelItems_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;

            var colorWheelItem = dgvColorWheelItems.Rows[e.RowIndex].DataBoundItem as ColorWheelItem;
            this.EditColorWheelItem(colorWheelItem, false);
        }


        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateEyedropper();
        }

        private void MainWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (_eyedropperActive)
                ckbEyedropper.Checked = false;
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            UpdateExportImageProperties();
            DrawBitmap();
        }

        #endregion
    }
}
