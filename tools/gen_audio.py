import numpy as np, wave
SR=22050
def save(name,x):
    x=np.clip(x,-1,1); d=(x*30000).astype('<i2')
    with wave.open(f'PrisonCookSimulator/Assets/Audio/{name}.wav','wb') as w:
        w.setnchannels(1);w.setsampwidth(2);w.setframerate(SR);w.writeframes(d.tobytes())
def t(s): return np.arange(int(SR*s))/SR
def env(n,a=0.005,r=8):
    e=np.ones(n);ai=max(1,int(a*SR));e[:ai]=np.linspace(0,1,ai)
    return e*np.exp(-np.arange(n)/SR*r)
rng=np.random.default_rng(27)
def note(f,dur,kind='sq',r=6,vol=.3):
    tt=t(dur);ph=2*np.pi*f*tt
    w={'sq':np.sign(np.sin(ph))*.6,'tri':2/np.pi*np.arcsin(np.sin(ph)),'sin':np.sin(ph)}[kind]
    return w*env(len(tt),r=r)*vol
tt=t(.12);save('station_hit',(np.sin(2*np.pi*(900-3000*tt)*tt)*.5+rng.normal(0,.15,len(tt)))*env(len(tt),r=35))
tt=t(.05);save('tick',np.sin(2*np.pi*1800*tt)*env(len(tt),a=.001,r=90)*.5)
tt=t(.6);n=rng.normal(0,1,len(tt));save('ruined',(np.convolve(n,np.ones(30)/30,'same')*1.5+np.sin(2*np.pi*(180-120*tt)*tt)*.5)*env(len(tt),r=5))
save('service_end',np.concatenate([note(f,.18,'tri',r=5,vol=.5) for f in (523,659,784)]+[note(1046,.5,'tri',r=4,vol=.5)]))
tt=t(.04);save('click',np.sin(2*np.pi*1200*tt)*env(len(tt),a=.001,r=120)*.4)
save('purchase',np.concatenate([note(1318,.07,'sq',r=20,vol=.25),note(1760,.25,'sq',r=10,vol=.25)]))
def track(bpm,prog,mel,seed,kick=True):
    beat=60/bpm; rng=np.random.default_rng(seed)
    total=int(SR*beat*4*len(prog)); out=np.zeros(total)
    def put(sig,start):
        i=int(start*SR); e=min(total,i+len(sig)); out[i:e]+=sig[:e-i]
    for b,root in enumerate(prog):
        for k in range(4):
            st=(b*4+k)*beat
            put(note(root/2,beat*.9,'tri',r=3,vol=.35),st)
            if kick: tk=t(.15); put(np.sin(2*np.pi*(120-400*tk)*tk)*env(len(tk),r=25)*.5,st)
            hn=rng.normal(0,1,int(.03*SR)); put(hn*env(len(hn),a=.001,r=150)*.08,st+beat/2)
            if rng.random()<.7:
                put(note(root*mel[rng.integers(len(mel))],beat*.45,'sq',r=9,vol=.12),st+(beat/2 if rng.random()<.4 else 0))
    return out/np.max(np.abs(out))*.8
save('music_menu',track(84,[220,174.6,196,164.8]*2,[1,1.2,1.5,1.78,2],1,kick=False))
save('music_line',track(128,[220,220,261.6,196]*3,[1,1.19,1.33,1.5,2],2))
