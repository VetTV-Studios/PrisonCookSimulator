from PIL import Image, ImageDraw, ImageFilter
import random
S=4; N=128*S
BG='#1C262E'; EDGE='#3A4650'; GOLD='#E8A838'; CREAM='#EDE6D6'; GREEN='#7FA18B'; RUST='#B5543A'; STEEL='#6B7A86'; BROWN='#7A4E2D'; DARK='#0E1216'; TRAY='#9AA7B0'
def c(v): return int(v*S)
def tile():
    im=Image.new('RGBA',(N,N),(0,0,0,0)); d=ImageDraw.Draw(im)
    d.rounded_rectangle([c(4),c(4),c(124),c(124)],c(18),fill=BG,outline=EDGE,width=c(3))
    return im,d
def grit(im,seed):
    r=random.Random(seed); d=ImageDraw.Draw(im)
    for _ in range(140):
        x,y=r.randint(c(10),c(118)),r.randint(c(10),c(118))
        d.point([(x,y)],fill=(14,18,22,90))
    return im
def tray(d): d.rounded_rectangle([c(18),c(70),c(110),c(108)],c(8),fill=TRAY,outline=DARK,width=c(3))
def bowl(d,fill): 
    d.pieslice([c(24),c(30),c(104),c(110)],0,180,fill=STEEL,outline=DARK,width=c(3))
    d.ellipse([c(26),c(56),c(102),c(80)],fill=fill,outline=DARK,width=c(2))
def blob(d,box,fill): d.ellipse([c(v) for v in box],fill=fill,outline=DARK,width=c(3))
def rect(d,box,fill,r=6): d.rounded_rectangle([c(v) for v in box],c(r),fill=fill,outline=DARK,width=c(3))
def save(im,path,seed):
    grit(im,seed).resize((128,128),Image.LANCZOS).save(f'PrisonCookSimulator/Assets/Art/{path}.png',optimize=True)
art={}
# ingredients
def ing_beef(d): blob(d,(28,36,100,96),RUST); [blob(d,(x,y,x+10,y+10),'#D98A7A') for x,y in((44,50),(66,60),(54,74),(78,46))]
def ing_chicken(d): blob(d,(30,30,90,84),GOLD); rect(d,(78,76,102,90),CREAM,4); blob(d,(96,70,110,84),CREAM)
def ing_beans(d): bowl(d,BROWN); [blob(d,(x,y,x+12,y+8),'#A0663F') for x,y in((40,60),(60,64),(78,60),(52,68))]
def ing_pasta(d): [d.arc([c(x),c(y),c(x+26),c(y+26)],200,340,fill=GOLD,width=c(8)) for x,y in((30,40),(62,36),(44,66),(74,66))]
def ing_bread(d): blob(d,(26,34,102,70),'#C98D4A'); rect(d,(26,52,102,98),'#C98D4A',10); rect(d,(36,60,92,90),CREAM,6)
def ing_veg(d): bowl(d,GREEN); [blob(d,(x,y,x+12,y+12),col) for x,y,col in((38,58,'#D9822B'),(60,60,GOLD),(78,58,'#D9822B'),(50,66,GREEN))]
def ing_potato(d): blob(d,(26,42,82,92),'#B88A55'); blob(d,(60,30,104,78),'#C79A62'); [blob(d,(x,y,x+5,y+5),BROWN) for x,y in((44,60),(76,48),(84,62))]
def ing_sauce(d): rect(d,(36,26,92,106),RUST,8); rect(d,(36,46,92,56),DARK,2); rect(d,(36,78,92,88),DARK,2); rect(d,(46,60,82,74),CREAM,3)
def ing_spice(d): rect(d,(44,22,84,38),STEEL,4); rect(d,(38,36,90,106),'#D9822B',10); rect(d,(46,56,82,80),CREAM,4)
def ing_egg(d): [blob(d,(x,34,x+30,94),CREAM) for x in (28,52,72)]
def ing_fish(d): 
    d.polygon([(c(26),c(64)),(c(70),c(36)),(c(96),c(64)),(c(70),c(92))],fill=STEEL,outline=DARK); d.polygon([(c(92),c(64)),(c(112),c(44)),(c(112),c(84))],fill=STEEL,outline=DARK); blob(d,(40,56,50,66),DARK)
def ing_prime(d): blob(d,(24,34,104,96),'#8E2F2A'); blob(d,(36,44,72,72),CREAM); d.line([c(40),c(86),c(96),c(50)],fill='#5E1E1A',width=c(4))
# recipes (on tray)
def r(fn):
    def g(d): tray(d); fn(d)
    return g
recs={
'joe':r(lambda d:(blob(d,(30,38,98,62),'#C98D4A'),rect(d,(30,52,98,64),RUST,4),blob(d,(30,60,98,80),'#C98D4A'))),
'mac':r(lambda d:(blob(d,(28,40,100,90),RUST),[d.arc([c(x),c(y),c(x+16),c(y+16)],200,340,fill=GOLD,width=c(5)) for x,y in((40,52),(60,46),(70,62),(48,66))])),
'loaf':r(lambda d:(rect(d,(30,40,98,82),'#6E5A48',8),d.text((c(56),c(46)),'?',fill=GOLD,font_size=c(30)))),
'beans':r(lambda d:(blob(d,(26,48,70,90),BROWN),rect(d,(70,46,104,82),'#C98D4A',6))),
'king':r(lambda d:(blob(d,(28,40,100,90),CREAM),[blob(d,(x,y,x+14,y+12),GOLD) for x,y in((42,54),(64,50),(56,68))],blob(d,(78,64,90,76),GREEN))),
'hash':r(lambda d:(blob(d,(28,44,100,90),'#B88A55'),blob(d,(48,44,80,74),CREAM),blob(d,(58,52,70,64),GOLD))),
'fish':r(lambda d:(rect(d,(28,44,74,70),GOLD,12),[rect(d,(x,50,x+8,86),'#E8C46A',2) for x in (78,88,98)])),
'veg':r(lambda d:(rect(d,(30,42,98,82),GREEN,8),[blob(d,(x,56,x+10,66),'#D9822B') for x in (44,62,80)])),
'stew':r(lambda d:(bowl(d,BROWN),[blob(d,(x,y,x+12,y+10),col) for x,y,col in((42,60,'#B88A55'),(62,62,RUST),(80,60,GREEN))])),
'warden':r(lambda d:(blob(d,(24,40,104,94),CREAM),blob(d,(38,48,82,80),'#8E2F2A'),blob(d,(80,56,96,72),GREEN),d.polygon([(c(64),c(16)),(c(70),c(30)),(c(84),c(30)),(c(72),c(38)),(c(78),c(50)),(c(64),c(42)),(c(50),c(50)),(c(56),c(38)),(c(44),c(30)),(c(58),c(30))],fill=GOLD,outline=DARK))),
}
# stations
def st_prep(d): rect(d,(22,60,106,96),'#8B6A45',6); d.polygon([(c(36),c(56)),(c(90),c(30)),(c(96),c(38)),(c(46),c(62))],fill=STEEL,outline=DARK); rect(d,(84,26,104,40),DARK,3)
def st_range(d): rect(d,(22,40,106,104),STEEL,6); [blob(d,(x,48,x+28,76),DARK) for x in (30,70)]; [d.arc([c(x+4),c(52),c(x+24),c(72)],0,360,fill=RUST,width=c(3)) for x in (30,70)]; [blob(d,(x,86,x+10,96),GOLD) for x in (36,58,80)]
def st_fryer(d): rect(d,(26,52,102,104),STEEL,6); rect(d,(32,46,96,62),GOLD,4); d.line([c(64),c(46),c(64),c(20)],fill=DARK,width=c(5)); [blob(d,(x,y,x+8,y+8),CREAM) for x,y in((40,36),(80,32),(60,30))]
def st_steam(d): rect(d,(20,62,108,100),STEEL,6); [rect(d,(x,54,x+24,70),TRAY,3) for x in (26,52,78)]; [d.arc([c(x),c(24),c(x+14),c(50)],90,270,fill=CREAM,width=c(4)) for x in (34,60,86)]
def st_window(d): rect(d,(18,24,110,104),DARK,6); rect(d,(28,34,100,74),'#2E3A44',4); rect(d,(18,74,110,86),STEEL,3); rect(d,(46,62,82,74),TRAY,3); blob(d,(84,40,96,52),GOLD)
items=[('ing_'+k,v) for k,v in dict(beef=ing_beef,chicken=ing_chicken,beans=ing_beans,pasta=ing_pasta,bread=ing_bread,veg=ing_veg,potato=ing_potato,sauce=ing_sauce,spice=ing_spice,egg=ing_egg,fish=ing_fish,prime=ing_prime).items()]
items+=[('recipe_'+k,v) for k,v in recs.items()]
items+=[('station_'+k,v) for k,v in dict(prep=st_prep,range=st_range,fryer=st_fryer,steam=st_steam,window=st_window).items()]
for i,(name,fn) in enumerate(items):
    im,d=tile(); fn(d); save(im,name,i)
# contact sheet for review
sheet=Image.new('RGBA',(128*9,128*3),DARK)
for i,(name,_) in enumerate(items): sheet.paste(Image.open(f'PrisonCookSimulator/Assets/Art/{name}.png'),((i%9)*128,(i//9)*128))
sheet.save('art_sheet.png')
print(len(items))
