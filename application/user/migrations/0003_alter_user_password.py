# Generated by Django 4.1.3 on 2023-02-05 12:26

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('user', '0002_alter_user_salt'),
    ]

    operations = [
        migrations.AlterField(
            model_name='user',
            name='password',
            field=models.CharField(help_text='密码', max_length=255, null=True, verbose_name='密码'),
        ),
    ]
