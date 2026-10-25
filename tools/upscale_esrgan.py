# Real-ESRGAN x4plus on CPU via spandrel, tiled. usage: upscale_esrgan.py model.pth in out
import sys, torch, numpy as np
from PIL import Image
from spandrel import ModelLoader
m=ModelLoader().load_from_file(sys.argv[1]).model.eval()
torch.set_num_threads(8)
img=np.asarray(Image.open(sys.argv[2]).convert('RGB')).astype(np.float32)/255
H,W,_=img.shape; T,P,S=256,16,4
out=np.zeros((H*S,W*S,3),np.float32)
with torch.no_grad():
    for y in range(0,H,T):
        for x in range(0,W,T):
            y0,x0=max(0,y-P),max(0,x-P); y1,x1=min(H,y+T+P),min(W,x+T+P)
            t=torch.from_numpy(img[y0:y1,x0:x1].transpose(2,0,1))[None]
            r=m(t)[0].clamp(0,1).numpy().transpose(1,2,0)
            ty,tx=(y-y0)*S,(x-x0)*S; h,w=min(T,H-y)*S,min(T,W-x)*S
            out[y*S:y*S+h,x*S:x*S+w]=r[ty:ty+h,tx:tx+w]
Image.fromarray((out*255+.5).astype(np.uint8)).save(sys.argv[3])
print('done',sys.argv[3],out.shape)
