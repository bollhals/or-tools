// Copyright 2010-2021 Google LLC
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Google.OrTools.Sat
{
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Google.OrTools.Util;
using Google.Protobuf.Collections;

public interface ILiteral
{
    ILiteral Not();
    int GetIndex();
}

internal static class HelperExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddOrIncrement(this Dictionary<IntVar, long> dict, IntVar key, long increment)
    {
#if NET6_0_OR_GREATER
        System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _) += increment;
#else
        if (dict.TryGetValue(key, out var value))
        {
            dict[key] = value + increment;
        }
        else
        {
            dict.Add(key, increment);
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TrySetCapacity<TField, TValues>(this RepeatedField<TField> field, IEnumerable<TValues> values)
    {
        if (values is ICollection<TValues> collection)
        {
            field.Capacity = collection.Count;
        }
    }
}

// Holds a linear expression.
public class LinearExpr
{
    public static LinearExpr Sum(IEnumerable<IntVar> vars)
    {
        return new SumArray(vars);
    }

    public static LinearExpr Sum(IEnumerable<LinearExpr> exprs)
    {
        return new SumArray(exprs);
    }

    public static LinearExpr WeightedSum(IEnumerable<IntVar> vars, IEnumerable<int> coeffs)
    {
        return new SumArray(vars, coeffs);
    }

    public static LinearExpr WeightedSum(IEnumerable<IntVar> vars, IEnumerable<long> coeffs)
    {
        return new SumArray(vars, coeffs);
    }

    public static LinearExpr Term(IntVar var, long coeff)
    {
        return Prod(var, coeff);
    }

    public static LinearExpr Affine(IntVar var, long coeff, long offset)
    {
        if (offset == 0)
        {
            return Prod(var, coeff);
        }
        else
        {
            return new SumArray(Prod(var, coeff), offset);
        }
    }

    public static LinearExpr Constant(long value)
    {
        return new ConstantExpr(value);
    }

    public int Index
    {
        get {
            return GetIndex();
        }
    }

    public virtual int GetIndex()
    {
        throw new NotImplementedException();
    }

    public virtual string ShortString()
    {
        return ToString();
    }

    public static LinearExpr operator +(LinearExpr a, LinearExpr b)
    {
        return new SumArray(a, b);
    }

    public static LinearExpr operator +(LinearExpr a, long v)
    {
        if (v == 0)
        {
            return a;
        }
        return new SumArray(a, v);
    }

    public static LinearExpr operator +(long v, LinearExpr a)
    {
        if (v == 0)
        {
            return a;
        }
        return new SumArray(a, v);
    }

    public static LinearExpr operator -(LinearExpr a, LinearExpr b)
    {
        return new SumArray(a, Prod(b, -1));
    }

    public static LinearExpr operator -(LinearExpr a, long v)
    {
        if (v == 0)
        {
            return a;
        }
        return new SumArray(a, -v);
    }

    public static LinearExpr operator -(long v, LinearExpr a)
    {
        if (v == 0)
        {
            return Prod(a, -1);
        }
        return new SumArray(Prod(a, -1), v);
    }

    public static LinearExpr operator *(LinearExpr a, long v)
    {
        return Prod(a, v);
    }

    public static LinearExpr operator *(long v, LinearExpr a)
    {
        return Prod(a, v);
    }

    public static LinearExpr operator -(LinearExpr a)
    {
        return Prod(a, -1);
    }

    public static BoundedLinearExpression operator ==(LinearExpr a, LinearExpr b)
    {
        return new BoundedLinearExpression(a, b, true);
    }

    public static BoundedLinearExpression operator !=(LinearExpr a, LinearExpr b)
    {
        return new BoundedLinearExpression(a, b, false);
    }

    public static BoundedLinearExpression operator ==(LinearExpr a, long v)
    {
        return new BoundedLinearExpression(a, v, true);
    }

    public static BoundedLinearExpression operator !=(LinearExpr a, long v)
    {
        return new BoundedLinearExpression(a, v, false);
    }

    public static BoundedLinearExpression operator >=(LinearExpr a, long v)
    {
        return new BoundedLinearExpression(v, a, Int64.MaxValue);
    }

    public static BoundedLinearExpression operator >=(long v, LinearExpr a)
    {
        return a <= v;
    }

    public static BoundedLinearExpression operator>(LinearExpr a, long v)
    {
        return new BoundedLinearExpression(v + 1, a, Int64.MaxValue);
    }

    public static BoundedLinearExpression operator>(long v, LinearExpr a)
    {
        return a < v;
    }

    public static BoundedLinearExpression operator <=(LinearExpr a, long v)
    {
        return new BoundedLinearExpression(Int64.MinValue, a, v);
    }

    public static BoundedLinearExpression operator <=(long v, LinearExpr a)
    {
        return a >= v;
    }

    public static BoundedLinearExpression operator<(LinearExpr a, long v)
    {
        return new BoundedLinearExpression(Int64.MinValue, a, v - 1);
    }

    public static BoundedLinearExpression operator<(long v, LinearExpr a)
    {
        return a > v;
    }

    public static BoundedLinearExpression operator >=(LinearExpr a, LinearExpr b)
    {
        return new BoundedLinearExpression(0, a - b, Int64.MaxValue);
    }

    public static BoundedLinearExpression operator>(LinearExpr a, LinearExpr b)
    {
        return new BoundedLinearExpression(1, a - b, Int64.MaxValue);
    }

    public static BoundedLinearExpression operator <=(LinearExpr a, LinearExpr b)
    {
        return new BoundedLinearExpression(Int64.MinValue, a - b, 0);
    }

    public static BoundedLinearExpression operator<(LinearExpr a, LinearExpr b)
    {
        return new BoundedLinearExpression(Int64.MinValue, a - b, -1);
    }

    public static LinearExpr Prod(LinearExpr e, long v)
    {
        if (v == 1)
        {
            return e;
        }
        else if (e is ProductCst p)
        {
            return new ProductCst(p.Expr, p.Coeff * v);
        }
        else
        {
            return new ProductCst(e, v);
        }
    }

    public static long GetVarValueMap(LinearExpr e, long initial_coeff, Dictionary<IntVar, long> dict)
    {
        List<LinearExpr> exprs = new List<LinearExpr>();
        List<long> coeffs = new List<long>();
        if (e is not null)
        {
            exprs.Add(e);
            coeffs.Add(initial_coeff);
        }
        long constant = 0;

        while (exprs.Count > 0)
        {
            LinearExpr expr = exprs[0];
            exprs.RemoveAt(0);
            long coeff = coeffs[0];
            coeffs.RemoveAt(0);
            if (coeff == 0 || expr is null)
                continue;

            if (expr is ProductCst p)
            {
                if (p.Coeff != 0)
                {
                    exprs.Add(p.Expr);
                    coeffs.Add(p.Coeff * coeff);
                }
            }
            else if (expr is SumArray a)
            {
                constant += coeff * a.Offset;
                foreach (LinearExpr sub in a.Expressions)
                {
                    if (sub is IntVar i)
                    {
                        dict.AddOrIncrement(i, coeff);
                    }
                    else if (sub is ProductCst sub_prod && sub_prod.Expr is IntVar sub_i)
                    {
                        dict.AddOrIncrement(sub_i, coeff * sub_prod.Coeff);
                    }
                    else
                    {
                        exprs.Add(sub);
                        coeffs.Add(coeff);
                    }
                }
            }
            else if (expr is ConstantExpr cte)
            {
                constant += coeff * cte.Value;
            }
            else if (expr is IntVar i)
            {
                dict.AddOrIncrement(i, coeff);
            }
            else if (expr is NotBooleanVariable not_bool)
            {
                IntVar bool_i = not_bool.NotVar();
                dict.AddOrIncrement(bool_i, -coeff);
                constant += coeff;
            }
            else
            {
                throw new ArgumentException("Cannot interpret '" + expr.ToString() + "' in an integer expression");
            }
        }
        return constant;
    }

    public static LinearExpr RebuildLinearExprFromLinearExpressionProto(LinearExpressionProto proto, CpModelProto model)
    {
        int numElements = proto.Vars.Count;
        long offset = proto.Offset;
        if (numElements == 0)
        {
            return LinearExpr.Constant(offset);
        }
        else if (numElements == 1)
        {
            IntVar var = new IntVar(model, proto.Vars[0]);
            long coeff = proto.Coeffs[0];
            return LinearExpr.Affine(var, coeff, offset);
        }
        else
        {
            LinearExpr[] exprs = new LinearExpr[numElements];
            for (int i = 0; i < numElements; ++i)
            {
                IntVar var = new IntVar(model, proto.Vars[i]);
                long coeff = proto.Coeffs[i];
                exprs[i] = Prod(var, coeff);
            }
            SumArray sum = new SumArray(exprs);
            sum.Offset = sum.Offset + offset;
            return sum;
        }
    }
}

public class ProductCst : LinearExpr
{
    public ProductCst(LinearExpr e, long v)
    {
        expr_ = e;
        coeff_ = v;
    }

    public LinearExpr Expr
    {
        get {
            return expr_;
        }
    }

    public long Coeff
    {
        get {
            return coeff_;
        }
    }

    private LinearExpr expr_;
    private long coeff_;
}

public class SumArray : LinearExpr
{
    public SumArray(LinearExpr a, LinearExpr b)
    {
        expressions_ = new List<LinearExpr>(2);
        AddExpr(a);
        AddExpr(b);
        offset_ = 0L;
    }

    public SumArray(LinearExpr a, long b)
    {
        expressions_ = new List<LinearExpr>(1);
        AddExpr(a);
        offset_ = b;
    }

    public SumArray(IEnumerable<LinearExpr> exprs)
    {
        expressions_ = new List<LinearExpr>(exprs);
        offset_ = 0L;
    }

    public SumArray(IEnumerable<IntVar> vars)
    {
        expressions_ = new List<LinearExpr>(vars);
        offset_ = 0L;
    }

    public SumArray(IntVar[] vars, long[] coeffs)
    {
        expressions_ = new List<LinearExpr>(vars.Length);
        for (int i = 0; i < vars.Length; ++i)
        {
            AddExpr(Prod(vars[i], coeffs[i]));
        }
        offset_ = 0L;
    }

    public SumArray(IEnumerable<IntVar> vars, IEnumerable<long> coeffs)
    {
        List<IntVar> tmp_vars = new List<IntVar>();
        foreach (IntVar v in vars)
        {
            tmp_vars.Add(v);
        }
        List<long> tmp_coeffs = new List<long>();
        foreach (long c in coeffs)
        {
            tmp_coeffs.Add(c);
        }
        if (tmp_vars.Count != tmp_coeffs.Count)
        {
            throw new ArgumentException("in SumArray(vars, coeffs), the two lists do not have the same length");
        }
        IntVar[] flat_vars = tmp_vars.ToArray();
        long[] flat_coeffs = tmp_coeffs.ToArray();
        expressions_ = new List<LinearExpr>(flat_vars.Length);
        for (int i = 0; i < flat_vars.Length; ++i)
        {
            expressions_.Add(Prod(flat_vars[i], flat_coeffs[i]));
        }
        offset_ = 0L;
    }

    public SumArray(IEnumerable<IntVar> vars, IEnumerable<int> coeffs)
    {
        List<IntVar> tmp_vars = new List<IntVar>();
        foreach (IntVar v in vars)
        {
            tmp_vars.Add(v);
        }
        List<long> tmp_coeffs = new List<long>();
        foreach (int c in coeffs)
        {
            tmp_coeffs.Add(c);
        }
        if (tmp_vars.Count != tmp_coeffs.Count)
        {
            throw new ArgumentException("in SumArray(vars, coeffs), the two lists do not have the same length");
        }
        IntVar[] flat_vars = tmp_vars.ToArray();
        long[] flat_coeffs = tmp_coeffs.ToArray();
        expressions_ = new List<LinearExpr>(flat_vars.Length);
        for (int i = 0; i < flat_vars.Length; ++i)
        {
            expressions_.Add(Prod(flat_vars[i], flat_coeffs[i]));
        }
        offset_ = 0L;
    }

    public void AddExpr(LinearExpr expr)
    {
        if (expr is not null)
        {
            expressions_.Add(expr);
        }
    }

    public List<LinearExpr> Expressions
    {
        get {
            return expressions_;
        }
    }

    public long Offset
    {
        get {
            return offset_;
        }
        set {
            offset_ = value;
        }
    }

    public override string ShortString()
    {
        return $"({ToString()})";
    }

    public override string ToString()
    {
        string result = "";
        foreach (LinearExpr expr in expressions_)
        {
            if (expr is null)
                continue;
            if (!String.IsNullOrEmpty(result))
            {
                result += " + ";
            }

            result += expr.ShortString();
        }
        if (offset_ != 0)
        {
            result += $" + {offset_}";
        }
        return result;
    }

    private List<LinearExpr> expressions_;
    private long offset_;
}

public class ConstantExpr : LinearExpr
{
    public ConstantExpr(long value)
    {
        value_ = value;
    }

    public long Value
    {
        get {
            return value_;
        }
    }

    public override string ShortString()
    {
        return value_.ToString();
    }

    public override string ToString()
    {
        return $"ConstantExpr({value_})";
    }

    private long value_;
}

public class IntVar : LinearExpr, ILiteral
{
    public IntVar(CpModelProto model, Domain domain, string name)
    {
        index_ = model.Variables.Count;
        var_ = new IntegerVariableProto();
        var_.Name = name;
        var_.Domain.AddRange(domain.FlattenedIntervals());
        model.Variables.Add(var_);
        negation_ = null;
    }

    public IntVar(CpModelProto model, long value, string name)
        : this(model, value, value, name)
    {
    }

    public IntVar(CpModelProto model, long lb, long ub, string name)
    {
        index_ = model.Variables.Count;
        var_ = new IntegerVariableProto();
        var_.Name = name;
        var_.Domain.Capacity = 2;
        var_.Domain.Add(lb);
        var_.Domain.Add(ub);
        model.Variables.Add(var_);
        negation_ = null;
    }

    public IntVar(CpModelProto model, int index)
    {
        index_ = index;
        var_ = model.Variables[index];
        negation_ = null;
    }

    public override int GetIndex()
    {
        return index_;
    }

    public IntegerVariableProto Proto
    {
        get {
            return var_;
        }
        set {
            var_ = value;
        }
    }

    public Domain Domain
    {
        get {
            return CpSatHelper.VariableDomain(var_);
        }
    }

    public override string ToString()
    {
        return var_.ToString();
    }

    public override string ShortString()
    {
        if (var_.Name is not null)
        {
            return var_.Name;
        }
        else
        {
            return var_.ToString();
        }
    }

    public string Name()
    {
        return var_.Name;
    }

    public ILiteral Not()
    {
        foreach (long b in var_.Domain)
        {
            if (b < 0 || b > 1)
            {
                throw new ArgumentException("Cannot call Not() on a non boolean variable");
            }
        }

        return negation_ ??= new NotBooleanVariable(this);
    }

    private readonly int index_;
    private IntegerVariableProto var_;
    private NotBooleanVariable negation_;
}

public class NotBooleanVariable : LinearExpr, ILiteral
{
    public NotBooleanVariable(IntVar boolvar)
    {
        boolvar_ = boolvar;
    }

    public override int GetIndex()
    {
        return -boolvar_.Index - 1;
    }

    public ILiteral Not()
    {
        return boolvar_;
    }

    public IntVar NotVar()
    {
        return boolvar_;
    }

    public override string ShortString()
    {
        return $"Not({boolvar_.ShortString()})";
    }

    private IntVar boolvar_;
}

public class BoundedLinearExpression
{
    public enum Type
    {
        BoundExpression,
        VarEqVar,
        VarDiffVar,
        VarEqCst,
        VarDiffCst,
    }

    public BoundedLinearExpression(long lb, LinearExpr expr, long ub)
    {
        left_ = expr;
        right_ = null;
        lb_ = lb;
        ub_ = ub;
        type_ = Type.BoundExpression;
    }

    public BoundedLinearExpression(LinearExpr left, LinearExpr right, bool equality)
    {
        left_ = left;
        right_ = right;
        lb_ = 0;
        ub_ = 0;
        type_ = equality ? Type.VarEqVar : Type.VarDiffVar;
    }

    public BoundedLinearExpression(LinearExpr left, long v, bool equality)
    {
        left_ = left;
        right_ = null;
        lb_ = v;
        ub_ = 0;
        type_ = equality ? Type.VarEqCst : Type.VarDiffCst;
    }

    bool IsTrue()
    {
        if (type_ == Type.VarEqVar)
        {
            return (object)left_ == (object)right_;
        }
        else if (type_ == Type.VarDiffVar)
        {
            return (object)left_ != (object)right_;
        }
        return false;
    }

    public static bool operator true(BoundedLinearExpression bie)
    {
        return bie.IsTrue();
    }

    public static bool operator false(BoundedLinearExpression bie)
    {
        return !bie.IsTrue();
    }

    public override string ToString()
    {
        switch (type_)
        {
        case Type.BoundExpression:
            return $"{lb_} <= {left_} <= {ub_}";
        case Type.VarEqVar:
            return $"{left_} == {right_}";
        case Type.VarDiffVar:
            return $"{left_} != {right_}";
        case Type.VarEqCst:
            return $"{left_} == {lb_}";
        case Type.VarDiffCst:
            return $"{left_} != {lb_}";
        default:
            throw new ArgumentException("Wrong mode in BoundedLinearExpression.");
        }
    }

    public static BoundedLinearExpression operator <=(BoundedLinearExpression a, long v)
    {
        if (a.CtType != Type.BoundExpression || a.Ub != Int64.MaxValue)
        {
            throw new ArgumentException("Operator <= not supported for this BoundedLinearExpression");
        }
        return new BoundedLinearExpression(a.Lb, a.Left, v);
    }

    public static BoundedLinearExpression operator<(BoundedLinearExpression a, long v)
    {
        if (a.CtType != Type.BoundExpression || a.Ub != Int64.MaxValue)
        {
            throw new ArgumentException("Operator < not supported for this BoundedLinearExpression");
        }
        return new BoundedLinearExpression(a.Lb, a.Left, v - 1);
    }

    public static BoundedLinearExpression operator >=(BoundedLinearExpression a, long v)
    {
        if (a.CtType != Type.BoundExpression || a.Lb != Int64.MinValue)
        {
            throw new ArgumentException("Operator >= not supported for this BoundedLinearExpression");
        }
        return new BoundedLinearExpression(v, a.Left, a.Ub);
    }

    public static BoundedLinearExpression operator>(BoundedLinearExpression a, long v)
    {
        if (a.CtType != Type.BoundExpression || a.Lb != Int64.MinValue)
        {
            throw new ArgumentException("Operator < not supported for this BoundedLinearExpression");
        }
        return new BoundedLinearExpression(v + 1, a.Left, a.Ub);
    }

    public LinearExpr Left
    {
        get {
            return left_;
        }
    }

    public LinearExpr Right
    {
        get {
            return right_;
        }
    }

    public long Lb
    {
        get {
            return lb_;
        }
    }

    public long Ub
    {
        get {
            return ub_;
        }
    }

    public Type CtType
    {
        get {
            return type_;
        }
    }

    private LinearExpr left_;
    private LinearExpr right_;
    private long lb_;
    private long ub_;
    private Type type_;
}

} // namespace Google.OrTools.Sat
