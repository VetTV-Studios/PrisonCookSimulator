from PIL import Image, ImageDraw, ImageFont, ImageFilter
import os
D='store-art'; F=f'{D}/fonts/BlackOpsOne-Regular.ttf'
ORANGE=(0xE8,0x68,0x1C); SHADOW=(0x11,0x13,0x15)
def src(n): return Image.open(f'{D}/source/{n}').convert('RGB')
def fit(im,w,h,anchor_y=0.5):
    r=max(w/im.width,h/im.height); im=im.resize((round(im.width*r),round(im.height*r)),Image.LANCZOS)
    x=(im.width-w)//2; y=int((im.height-h)*anchor_y); return im.crop((x,y,x+w,y+h))
def title(im,box):
    """Draw PRISON COOK / SIMULATOR centred inside box=(x0,y0,x1,y1)."""
    x0,y0,x1,y1=box; bw,bh=x1-x0,y1-y0; d=ImageDraw.Draw(im)
    s=int(bh*0.62)
    while True:
        f1=ImageFont.truetype(F,s); f2=ImageFont.truetype(F,int(s*0.48))
        w1=d.textbbox((0,0),'PRISON COOK',font=f1)[2]; w2=d.textbbox((0,0),'SIMULATOR',font=f2)[2]
        h=s+int(s*0.48)+int(s*0.12)
        if w1<=bw and h<=bh: break
        s-=2
    sw=max(2,s//14); top=y0+(bh-h)//2
    # soft dark glow for legibility
    glow=Image.new('L',im.size,0); g=ImageDraw.Draw(glow)
    g.text((x0+(bw-w1)//2,top),'PRISON COOK',font=f1,fill=200,stroke_width=sw*4)
    g.text((x0+(bw-w2)//2,top+s+int(s*0.12)),'SIMULATOR',font=f2,fill=200,stroke_width=sw*4)
    im.paste(SHADOW,(0,0),glow.filter(ImageFilter.GaussianBlur(sw*3)))
    d=ImageDraw.Draw(im)
    d.text((x0+(bw-w1)//2,top),'PRISON COOK',font=f1,fill=ORANGE,stroke_width=sw,stroke_fill=SHADOW)
    d.text((x0+(bw-w2)//2,top+s+int(s*0.12)),'SIMULATOR',font=f2,fill=ORANGE,stroke_width=max(2,sw//2+1),stroke_fill=SHADOW)
    return im
def save(im,name): im.convert('RGB').save(f'{D}/{name}',optimize=True); print(name,im.size)
key,poster,hero=src('key_art.jpg'),src('poster_art.jpg'),src('hero_art.jpg')
# itch cover 630x500, title inside central 560x440 safe area (top band)
im=fit(key,630,500,0.35); save(title(im,(55,40,575,190)),'itch_cover_630x500.png')
# Store box 1:1
im=fit(key,2160,2160); save(title(im,(260,140,1900,700)),'store_box_2160x2160.png')
save(Image.open(f'{D}/store_box_2160x2160.png').resize((1080,1080),Image.LANCZOS),'store_box_1080x1080.png')
# poster 2:3, title in top
im=fit(poster,1440,2160,0.0); save(title(im,(120,90,1320,560)),'store_poster_1440x2160.png')
save(Image.open(f'{D}/store_poster_1440x2160.png').resize((720,1080),Image.LANCZOS),'store_poster_720x1080.png')
# hero 1920x1080 no title
save(fit(hero,1920,1080),'store_hero_1920x1080.png')
# itch banner 960x300 from hero, title on dark left side
im=fit(hero,960,300,0.3); save(title(im,(30,30,470,270)),'itch_banner_960x300.png')
