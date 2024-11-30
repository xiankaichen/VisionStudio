# 基础镜像
FROM python:3.9
# 镜像作者
MAINTAINER 管理员
# 设置 python 环境变量
ENV PYTHONUNBUFFERED 1
ENV PYTHONDONTWRITEBYTECODE 1

# 设置时区
ENV TZ Asia/Shanghai

# 更新pip
RUN pip install -U pip
# 设置国内镜像源
RUN pip config set global.index-url http://mirrors.aliyun.com/pypi/simple
RUN pip config set install.trusted-host mirrors.aliyun.com
# 升级pip版本
RUN python -m pip install --upgrade pip
# 安装工具
RUN pip install setuptools

# 创建容器工作目录
RUN mkdir -p /data/apps
# 设置容器内工作目录
WORKDIR /data/apps

# 将当前主机目录全部文件复制至容器工作目录
ADD . /data/apps
# 也可以单个拷贝
# COPY ./requirements.txt .

# 安装依赖
RUN pip install -r requirements.txt
RUN pip install uwsgi