﻿module Gjallarhorn.Tests.Observable

open Gjallarhorn

open System
open NUnit.Framework

[<TestCase(1, 2)>]
[<TestCase(42, 32)>]
[<TestCase(Int32.MinValue, Int32.MaxValue)>]
let ``Mutation triggers IObservable`` (start : int) finish =
    let result = Mutable.create start
    let obs = result |> View.asObservable
    
    let changedValue = ref result.Value
    use subscription = 
        obs |> Observable.subscribe((fun i -> changedValue := i))
    
    result.Value <- finish
    Assert.AreEqual(finish, !changedValue)

[<TestCase(1, 2, "2")>]
[<TestCase(42, 32, "32")>]
[<TestCase(Int32.MinValue, Int32.MaxValue, "2147483647")>]
let ``View triggers IObservable`` (start : int) (finish:int) (viewFinish: string) =
    let result = Mutable.create start
    let view = View.map (fun i -> i.ToString()) result
    let obs = view |> View.asObservable
    
    let changedValue = ref view.Value
    use subscription = 
        obs |> Observable.subscribe((fun s -> changedValue := s))
        
    result.Value <- finish
    Assert.AreEqual(viewFinish, !changedValue)

[<TestCase(1, 2)>]
[<TestCase(42, 32)>]
[<TestCase(Int32.MinValue, Int32.MaxValue)>]
let ``Observable Dispose stops tracking`` (start:int) finish =
    let result = Mutable.create start    
    let obs = result |> View.asObservable
    
    let changedValue = ref result.Value
    let subscription = 
        obs |> Observable.subscribe((fun i -> changedValue := i))
        
    // Should track/change
    result.Value <- finish
    Assert.AreEqual(finish, !changedValue)
    subscription.Dispose()
    
    // Should not track/change anymore
    result.Value <- start
    Assert.AreEqual(finish, !changedValue)
 
[<Test;TestCaseSource(typeof<Utilities>,"CasesStart")>]
let ``View\subscribeToObservable initializes properly`` start =
    let evt = Event<'a>()
    let obs = evt.Publish

    let view, handle = View.subscribeFromObservable start obs
    Assert.AreEqual(box start, view.Value)
    handle.Dispose()

[<Test;TestCaseSource(typeof<Utilities>,"CasesStartEnd")>]
let ``View\subscribeToObservable tracks changes in values`` start finish =
    let evt = Event<_>()
    let obs = evt.Publish

    let view, handle = View.subscribeFromObservable start obs 
    Assert.AreEqual(box start, view.Value)

    evt.Trigger finish
    Assert.AreEqual(box finish, view.Value)
    handle.Dispose()
