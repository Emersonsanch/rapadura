import re,os,subprocess,sys
S=sys.argv[1]; D='Library/Bee/artifacts/1900b0aE.dag/'
DOTNET=r'C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Data\NetCoreRuntime\dotnet.exe'
CSC=r'C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Data\DotNetSdkRoslyn\csc.dll'
new=subprocess.run(['git','ls-files','--others','--exclude-standard','*.cs'],capture_output=True,text=True).stdout.split()
def build(name,prefixes,extra_sub={}):
    lines=open(D+name+'.rsp',encoding='utf8').read().splitlines()
    out=[]
    for l in lines:
        m=re.match(r'^"(.*\.cs)"$',l)
        if m and not os.path.exists(m.group(1)): continue
        if l.startswith('-out:'): l='-out:"%s/%s.dll"'%(S,name)
        if l.startswith('-refout:'): l='-refout:"%s/%s.ref.dll"'%(S,name)
        for k,v in extra_sub.items(): l=l.replace(k,v)
        out.append(l)
    for f in new:
        if any(f.startswith(p) for p in prefixes) and '"%s"'%f not in out: out.append('"%s"'%f)
    p=S+'/'+name+'.rsp'; open(p,'w',encoding='utf8').write('\n'.join(out))
    r=subprocess.run([DOTNET,'exec',CSC,'/nostdlib','/noconfig','@'+p],capture_output=True,text=True,encoding='utf8',errors='replace')
    errs=[x for x in r.stdout.splitlines() if 'error' in x]
    print(name,'errors:',len(errs)); print('\n'.join(errs[:40]))
build('Rapadura.Runtime',['Assets/Scripts/'])
build('Rapadura.Tests.EditMode',['Assets/Tests/EditMode/'],{D+'Rapadura.Runtime.ref.dll':S+'/Rapadura.Runtime.ref.dll','Library/Bee/artifacts/1900b0aE.dag/Rapadura.Runtime.ref.dll':S+'/Rapadura.Runtime.ref.dll'})
