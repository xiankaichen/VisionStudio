# Generated by Django 4.1.3 on 2023-02-05 12:19

from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('user', '0001_initial'),
    ]

    operations = [
        migrations.AlterField(
            model_name='user',
            name='salt',
            field=models.CharField(help_text='加密盐', max_length=30, null=True, verbose_name='加密盐'),
        ),
    ]