# Generated by Django 4.1.3 on 2023-02-05 11:10

from django.db import migrations, models


class Migration(migrations.Migration):

    initial = True

    dependencies = [
    ]

    operations = [
        migrations.CreateModel(
            name='AdSort',
            fields=[
                ('id', models.AutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='主键ID')),
                ('create_user', models.IntegerField(default=0, verbose_name='创建人')),
                ('create_time', models.DateTimeField(auto_now_add=True, max_length=11, null=True, verbose_name='创建时间')),
                ('update_user', models.IntegerField(default=0, verbose_name='更新人')),
                ('update_time', models.DateTimeField(auto_now=True, max_length=11, null=True, verbose_name='更新时间')),
                ('is_delete', models.BooleanField(default=0, verbose_name='逻辑删除')),
                ('name', models.CharField(help_text='广告位名称', max_length=255, verbose_name='广告位名称')),
                ('item_id', models.IntegerField(default=0, help_text='站点ID', verbose_name='站点ID')),
                ('cate_id', models.IntegerField(default=0, help_text='栏目ID', verbose_name='栏目ID')),
                ('loc_id', models.IntegerField(default=0, help_text='广告位位置', verbose_name='广告位位置')),
                ('platform', models.IntegerField(choices=[(1, 'PC站'), (2, 'WAP站'), (3, '微信小程序'), (4, 'APP应用')], default=0, help_text='投放平台：1PC站 2WAP站 3微信小程序 4APP应用', verbose_name='投放平台：1PC站 2WAP站 3微信小程序 4APP应用')),
                ('description', models.CharField(help_text='广告位描述', max_length=255, null=True, verbose_name='广告位描述')),
                ('sort', models.IntegerField(default=0, help_text='广告位排序', verbose_name='广告位排序')),
            ],
            options={
                'verbose_name': '广告位表',
                'verbose_name_plural': '广告位表',
                'db_table': 'django_ad_sort',
            },
        ),
    ]
