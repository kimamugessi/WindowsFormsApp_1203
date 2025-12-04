using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JYVision.UIControl
{
    public partial class ImageViewCtrl : UserControl
    {
        private bool _isInitialized = false;

        private Bitmap _bitmapImage = null; //로드된 이미지

        private Bitmap Canvas= null;    //화면 깜빡이는 현상 없애기 위함, 숨겨진 상태에서 그리고 나중에 보여지는 기능

        private RectangleF ImageRect = new RectangleF(0, 0, 0, 0);  //표시 이미지 크기 및 위치

        private float _curZoom = 1.0f;  //배율
        private float _zoomFactor = 1.1f;   //확대,축소 변경 단위
        private float MinZoom = 1.0f;   //Zoom 최소 크기
        private float MaxZoom = 100.0f; //Zoom 최대 크기

        public ImageViewCtrl()
        {
            InitializeComponent();
            initializeCanvas();

            MouseWheel += new MouseEventHandler(ImageViewCCtrl_MouseWheel);
        }

        private void initializeCanvas() //캔버스 초기화 및 설정
        {
            ResizeCanvas(); //캔버스 userControl 크기만큼 생성
            DoubleBuffered = true;  //깜빡임 방지 더블 버퍼 설정
        }
        private void ResizeCanvas() //도킹펜이 변할때마다 이미지 사이즈 재계산을 위함
        {
            if (Width <= 0 || Height <= 0 || _bitmapImage == null) return;
            Canvas=new Bitmap(Width, Height);
            if(Canvas==null) return;

            float virtualWidth = _bitmapImage.Width * _curZoom;
            float virtualHeight = _bitmapImage.Height * _curZoom;

            float offsetX = virtualWidth < Width ? (Width - virtualWidth) / 2f : 0f;
            float offsetY = virtualHeight < Height ? (Height - virtualHeight) / 2f : 0f;

            ImageRect=new RectangleF(offsetX, offsetY, virtualWidth, virtualHeight);
        }

        public void LoadBitmap(Bitmap bitmap)   //이미지 로드 함수
        {
            if (_bitmapImage != null)   //이미지가 있다면 해제 후 초기화
            {
                //이미지 크기가 같다면 이미지 변경 후 화면 갱신
                if (_bitmapImage.Width == bitmap.Width && _bitmapImage.Height == bitmap.Height) 
                {
                    _bitmapImage=bitmap;
                    Invalidate();
                    return;
                }
                _bitmapImage.Dispose(); //birmap 객체가 사요하던 메모리 리소스 해제
                _bitmapImage = null;  //객체 해제하여 GC을 수집할 수 있도록 설정
            }
            _bitmapImage = bitmap;  //새 이미지 로드;
            if (_isInitialized == false)    ////bitmap==null 예외처리도 초기화되지않은 변수들 초기화
            {
                _isInitialized = true;
                ResizeCanvas();
            }
            FitImageToScreen();
        }

        private void FitImageToScreen()
        {
            RecalcZoomRatio();

            float NewWidth = _bitmapImage.Width * _curZoom; //bitmap이미지*배율
            float NewHeight = _bitmapImage.Height * _curZoom;

            ImageRect = new RectangleF( //이미지가 UserControl중앙에 배치되도록 정렬
                (Width - NewWidth) / 2,
                (Height - NewHeight) / 2,
                NewWidth,
                NewHeight
            );

            Invalidate();   //내부 함수, 화면 갱신 기능
        }
        private void RecalcZoomRatio()  //줌비율 재계산(모르것음)
        {
            if (_bitmapImage == null || Width <= 0 || Height <= 0) return;

            Size imageSize = new Size(_bitmapImage.Width, _bitmapImage.Height);

            float aspectRatio = (float)imageSize.Height / (float)imageSize.Width;
            float clientAspect = (float)Height / (float)Width;

            float ratio;

            if (aspectRatio <= clientAspect)
                ratio = (float)Width / (float)imageSize.Width;
            else
                ratio = (float)Height / (float)imageSize.Height;

            float minZoom = ratio;

            MinZoom = minZoom;

            _curZoom = Math.Max(MinZoom, Math.Min(MaxZoom, ratio)); //min, max값을 벗어나지 않게 설정

            Invalidate();   //내부 함수, 화면 갱신 기능
        }

        // Windows Forms에서 컨트롤이 다시 그려질 때 자동으로 호출되는 메서드
        // 화면새로고침(Invalidate()), 창 크기변경, 컨트롤이 숨겨졌다가 나타날때 실행
        protected override void OnPaint(PaintEventArgs e)
        {
           base.OnPaint(e); //base.____:부모 클래스의 것을 가져다 씀

            if (_bitmapImage != null && Canvas != null)
            {
                using(Graphics g = Graphics.FromImage(Canvas))  //캔버스 초기화, 이미지 그리기
                {
                    g.Clear(Color.Transparent); //배경을 투명하게

                    g.InterpolationMode = InterpolationMode.NearestNeighbor;    //이미지 확대or축소때 화질 최적화 방식(Interpolation Mode) 설정   
                    g.DrawImage(_bitmapImage, ImageRect);
                    e.Graphics.DrawImage(Canvas, 0, 0); // 캔버스를 UserControl 화면에 표시
                }
            }
        }
        //위자드가 없어서 ImageViewCtrl에 직접 작성해서 생성
        //휠을 움직일 때 
        private void ImageViewCCtrl_MouseWheel(object sender, MouseEventArgs e) 
        {
            if (e.Delta < 0) ZoomMove(_curZoom/_zoomFactor,e.Location); //휠을 아래로 내렸을 때 ZomMove 함수 실행
            else ZoomMove(_curZoom*_zoomFactor,e.Location); //위로 올렸을 때 ZomMove 함수 실행

            if (_bitmapImage != null){   //새 이미지 위치 반영?
                ImageRect.Width = _bitmapImage.Width * _curZoom;
                ImageRect.Height = _bitmapImage.Height * _curZoom;
            }
            Invalidate();   //내부 함수, 화면 갱신 기능
        }
        private void ZoomMove(float zoom,Point zoomOrigin)  //Zoom 확대/축소 값 계산
        {
            PointF virtualOrigin=ScreenToVirtual(new PointF(zoomOrigin.X, zoomOrigin.Y));

            _curZoom = Math.Max(MinZoom, Math.Min(MaxZoom, zoom));  //Min, Max 값을 벗어나지 않게 설정
            if (_curZoom <= MinZoom) return;

            PointF zoomedOrigin = VirtualToScreen(virtualOrigin);   //?

            float dx=zoomedOrigin.X - zoomOrigin.X;     //?
            float dy=zoomedOrigin .Y - zoomOrigin.Y;    //?

            ImageRect.X -= dx;  //?
            ImageRect.Y-=dy;    //?
        }
        #region 좌표계 변환
        private PointF GetScreenOffset() //특정 지점이 화면 상에 얼마나 떨어져있는지
        {
            return new PointF(ImageRect.X, ImageRect.Y);
        }
        private PointF ScreenToVirtual(PointF screenPos) //창 외곽: Screen, 이미지 외곽: Virtual
        {
            PointF offset = GetScreenOffset();
            return new PointF(
                (screenPos.X-offset.X)/_curZoom,
                (screenPos.Y - offset.Y)/_curZoom);

        }

        private PointF VirtualToScreen(PointF virtualPos)   //창 외곽: Screen, 이미지 외곽: Virtual
        {
            PointF offset = GetScreenOffset();
            return new PointF(
                virtualPos.X *_curZoom + offset.X,
                virtualPos.Y * _curZoom + offset.Y);

        }
        #endregion

        private void ImageViewCtrl_MouseDoubleClick(object sender, MouseEventArgs e)    //마우스 더블 클릭(버튼 상관 없음) 시 이미지 크기 맞춤
        {
            FitImageToScreen();
        }

        private void ImageViewCtrl_Resize(object sender, EventArgs e)   //다시 사이즈 계산 후 갱신
        {
            ResizeCanvas();
            Invalidate();
        }
    }
}
