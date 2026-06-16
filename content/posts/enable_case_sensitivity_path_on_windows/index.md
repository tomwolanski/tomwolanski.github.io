---
title: "Enable case sensitive paths in Windows 11"
date: 2026-06-11
cover : "image.jpeg"
image : "image.jpeg"
featuredImage: "image.jpeg"
useRelativeCover : true
draft: true
tags:
  - general
  - Windows
summary: "Fixing git issues with a case sensitive paths."
---





```bash
 error: You're on a case-insensitive filesystem, and the remote you are
trying to fetch from has references that only differ in casing. It
is impossible to store such references with the 'files' backend. You
can either accept this as-is, in which case you won't be able to
store all remote references on disk. Or you can alternatively
migrate your repository to use the 'reftable' backend with the
following command:

    git refs migrate --ref-format=reftable

Please keep in mind that not all implementations of Git support this
new format yet. So if you use tools other than Git to access this
repository it may not be an option to migrate to reftables.

error: fetching ref refs/remotes/origin/features/abcd failed: reference conflict due to case-insensitive filesystem
error: fetching ref refs/remotes/origin/features/xyz: reference conflict due to case-insensitive filesystem
```






### Step 1
Rename your original path to something else
```
mv C:\DEV\git C:\DEV\git_back
```

### Step 3
Create new empty directory with the same name as your old directory
```
mkdir C:\DEV\git
```

### Step 3
Enable support for case-sensitive paths (needs to be done as administrator)
```
fsutil.exe file setCaseSensitiveInfo C:\DEV\git enable   
```
### Step 4
Copy your files to the new directory
```
Get-ChildItem C:\DEV\git_back | Copy-Item -Destination C:\DEV\git -Recurse   
```

### verify 
```
to verify:
 fsutil.exe file queryCaseSensitiveInfo C:\DEV\git\some\path\in\any\project
Case sensitive attribute on directory C:\DEV\git\some\path\in\any\project is enabled.
```