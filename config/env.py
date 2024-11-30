# +----------------------------------------------------------------------
# | DjangoAdmin敏捷开发框架 [ 赋能开发者，助力企业发展 ]
# +----------------------------------------------------------------------
# | 版权所有 2021~2024 北京DjangoAdmin研发中心
# +----------------------------------------------------------------------
# | Licensed Apache-2.0 DjangoAdmin并不是自由软件，未经许可禁止去掉相关版权
# +----------------------------------------------------------------------
# | 官方网站: https://www.djangoadmin.cn
# +----------------------------------------------------------------------
# | 作者: @一米阳光 团队荣誉出品
# +----------------------------------------------------------------------
# | 版权和免责声明:
# | 本团队对该软件框架产品拥有知识产权（包括但不限于商标权、专利权、著作权、商业秘密等）
# | 均受到相关法律法规的保护，任何个人、组织和单位不得在未经本团队书面授权的情况下对所授权
# | 软件框架产品本身申请相关的知识产权，禁止用于任何违法、侵害他人合法权益等恶意的行为，禁
# | 止用于任何违反我国法律法规的一切项目研发，任何个人、组织和单位用于项目研发而产生的任何
# | 意外、疏忽、合约毁坏、诽谤、版权或知识产权侵犯及其造成的损失 (包括但不限于直接、间接、
# | 附带或衍生的损失等)，本团队不承担任何法律责任，本软件框架禁止任何单位和个人、组织用于
# | 任何违法、侵害他人合法利益等恶意的行为，如有发现违规、违法的犯罪行为，本团队将无条件配
# | 合公安机关调查取证同时保留一切以法律手段起诉的权利，本软件框架只能用于公司和个人内部的
# | 法律所允许的合法合规的软件产品研发，详细声明内容请阅读《框架免责声明》附件；
# +----------------------------------------------------------------------

import os

# ======================= 应用配置 ==========================

# 应用名称
DJANGO_NAME = os.getenv('DJANGO_NAME', 'VisionStudio')
# 应用版本
DJANGO_VERSION = os.getenv('DJANGO_VERSION', 'v2.5.0')
# 开启DEBUG调试
DJANGO_DEBUG = (os.getenv('DJANGO_DEBUG', 'True') == 'True')
# 演示模式：是-True,否-False
DJANGO_DEMO = (os.getenv('DJANGO_DEMO', 'True') == 'True')
# 应用根目录
DJANGO_ROOT_PATH = os.path.abspath(os.path.dirname(os.path.dirname(__file__)))
# 应用模板路径
DJANGO_TEMPLATE_FOLDER = os.path.join(DJANGO_ROOT_PATH, '/templates')
# 应用静态资源路径
DJANGO_STATIC_FOLDER = os.path.join(DJANGO_ROOT_PATH, '/public/static')
# 应用文件存储路径
DJANGO_UPLOAD_DIR = os.getenv('DJANGO_UPLOAD_DIR', os.path.join(DJANGO_ROOT_PATH, '/public/uploads'))
# 正式图片路径
DJANGO_IMAGE_PATH = DJANGO_UPLOAD_DIR + '/images'
# 临时文件路径
DJANGO_TEMP_PATH = DJANGO_UPLOAD_DIR + '/temp'
# 应用图片域名
DJANGO_IMAGE_URL = os.getenv('DJANGO_IMAGE_URL', 'http://images.django.elevue')

# ======================= 数据库配置 ==========================

# 数据库 ENGINE ，默认演示使用 sqlite3 数据库，正式环境建议使用 mysql 数据库
# sqlite3 设置
# DATABASE_ENGINE = "django.db.backends.sqlite3"
# DATABASE_NAME = os.path.join(BASE_DIR, "db.sqlite3")

# 使用mysql时，改为此配置
DATABASE_ENGINE = os.getenv('DATABASE_ENGINE', "django.db.backends.mysql")
# 数据库库名
DATABASE_NAME = os.getenv('DATABASE_NAME', 'djangoadmin.django.elevue')
# 数据库地址 改为自己数据库地址
DATABASE_HOST = os.getenv('DATABASE_HOST', "127.0.0.1")
# 数据库端口
DATABASE_PORT = os.getenv('DATABASE_PORT', 3306)
# 数据库用户名
DATABASE_USER = os.getenv('DATABASE_USER', 'root')
# 数据库密码
DATABASE_PASSWORD = os.getenv('DATABASE_PASSWORD', '123456')
# 数据表前缀
DATABASE_PREFIX = os.getenv('DATABASE_PREFIX', "django_")
