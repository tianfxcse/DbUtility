﻿using Ivony.Data.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Data.Queries
{

  /// <summary>
  /// 辅助构建 ParameterizedQuery 对象
  /// </summary>
  public sealed class ParameterizedQueryBuilder
  {

    private StringBuilder textBuilder = new StringBuilder();

    private List<DbParameter> parameters = new List<DbParameter>();


    private object _sync = new object();


    /// <summary>
    /// 用于同步的对象
    /// </summary>
    public object SyncRoot
    {
      get { return _sync; }
    }


    /// <summary>
    /// 添加一段查询文本
    /// </summary>
    /// <param name="text">要添加到末尾的查询文本</param>
    public void AppendText( string text )
    {
      lock ( _sync )
      {
        textBuilder.Append( text.Replace( "#", "##" ) );
      }
    }


    /// <summary>
    /// 添加一个字符到查询文本
    /// </summary>
    /// <param name="ch">要添加到查询文本末尾的字符</param>
    public void Append( char ch )
    {
      lock ( _sync )
      {
        if ( ch == '#' )
          textBuilder.Append( "##" );

        else
          textBuilder.Append( ch );
      }
    }

    /// <summary>
    /// 添加一个查询参数
    /// </summary>
    /// <param name="value">参数值</param>
    public void AppendParameter( object value )
    {
      var partial = value as IParameterizedQueryPartial;
      if ( partial != null )
        AppendPartial( partial );

      lock ( _sync )
      {

        var p = value as DbParameter;
        if ( p != null )
          AppendParameter( p );

        else
          AppendParameter( CreateParameter( value ) );
      }
    }


    /// <summary>
    /// 添加一个查询参数
    /// </summary>
    /// <param name="parameter">查询参数</param>
    public void AppendParameter( DbParameter parameter )
    {
      lock ( _sync )
      {
        parameters.Add( parameter );
        textBuilder.AppendFormat( "#{0}#", parameters.Count - 1 );
      }
    }



    private static DbParameter CreateParameter( object value )
    {
      var dbValue = DbValueConverter.ConvertTo( value );
      var type = DbDataType.GetType( dbValue );


      return new DbParameter( null, dbValue, type, ParameterDirection.Input );

    }




    /// <summary>
    /// 创建参数化查询对象实例
    /// </summary>
    /// <returns>参数化查询对象</returns>
    public ParameterizedQuery CreateQuery()
    {
      lock ( _sync )
      {
        return new ParameterizedQuery( textBuilder.ToString(), parameters.ToArray() );
      }
    }


    /// <summary>
    /// 在当前位置添加一个部分查询
    /// </summary>
    /// <param name="partial">要添加的部分查询对象</param>
    public void AppendPartial( IParameterizedQueryPartial partial )
    {
      lock ( _sync )
      {
        partial.AppendTo( this );
      }

    }



    internal bool IsEndWithWhiteSpace()
    {
      lock ( _sync )
      {
        return char.IsWhiteSpace( textBuilder[textBuilder.Length - 1] );
      }
    }
  }
}
